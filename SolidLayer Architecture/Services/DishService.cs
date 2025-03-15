using Microsoft.Data.SqlClient;
using SolidLayer_Architecture.Helpers;
using SolidLayer_Architecture.Repositories;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Services
{
    /// <summary>
    /// Service for dish business logic and operations
    /// </summary>
    public class DishService : IDishService
    {
        private readonly IRepository<Dish> _dishRepository;
        private readonly ILogger<DishService> _logger;

        public DishService(IRepository<Dish> dishRepository, ILogger<DishService> logger)
        {
            _dishRepository = dishRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all available dishes
        /// </summary>
        public IEnumerable<Dish> GetAllDishes()
        {
            try
            {
                var dishes = _dishRepository.GetAll().ToList();
                // Convert health factor numeric values to display strings
                foreach (var dish in dishes)
                {
                    ConvertHealthFactorForDisplay(dish);
                }
                _logger.LogInformation("Retrieved {count} dishes", dishes.Count());
                return dishes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all dishes");
                return Enumerable.Empty<Dish>();
            }
        }

        /// <summary>
        /// Gets a specific dish by ID
        /// </summary>
        public Dish? GetDishById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("GetDishById called with null or empty ID");
                return null;
            }
            
            try
            {
                var dish = _dishRepository.GetById(id);
                if (dish != null)
                {
                    ConvertHealthFactorForDisplay(dish);
                }
                if (dish == null)
                {
                    _logger.LogWarning("No dish found with ID: {dishId}", id);
                }
                return dish;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dish by ID: {dishId}", id);
                return null;
            }
        }

        /// <summary>
        /// Adds a new dish to the system
        /// </summary>
        public void AddDish(Dish dish)
        {
            try
            {
                ValidateAndPrepareDish(dish);
                
                // Ensure the dish has a valid ID
                if (string.IsNullOrEmpty(dish.DishID))
                {
                    dish.DishID = GenerateUniqueId();
                }

                // Health factor is already converted to numeric value in page model
                _logger.LogInformation("Adding dish: {dishId} - {name}", dish.DishID, dish.Name);
                _dishRepository.Insert(dish);
                _logger.LogInformation("Dish added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding dish: {dishId} - {name}", dish.DishID, dish.Name);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing dish
        /// </summary>
        public void UpdateDish(Dish dish)
        {
            try
            {
                ValidateAndPrepareDish(dish);
                
                // Ensure we have numeric health factor value before saving
                if (!string.IsNullOrEmpty(dish.HealthFactor) && !int.TryParse(dish.HealthFactor, out _))
                {
                    int healthFactorValue = HealthFactorHelper.ToInt(dish.HealthFactor);
                    dish.HealthFactor = healthFactorValue.ToString();
                }

                _logger.LogInformation("Updating dish: {dishId}", dish.DishID);
                _dishRepository.Update(dish);
                _logger.LogInformation("Dish updated successfully: {dishId}", dish.DishID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish: {dishId}", dish.DishID);
                throw;
            }
        }

        /// <summary>
        /// Deletes a dish by ID
        /// </summary>
        public void DeleteDish(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Dish ID cannot be null or empty", nameof(id));
                }
                
                _logger.LogInformation("Deleting dish: {dishId}", id);
                _dishRepository.Delete(id);
                _logger.LogInformation("Dish deleted successfully: {dishId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dish: {dishId}", id);
                throw;
            }
        }

        /// <summary>
        /// Searches for dishes by name or description
        /// </summary>
        public IEnumerable<Dish> SearchDishes(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return GetAllDishes();
                }
                
                string sql = "SELECT * FROM DISHES WHERE Name LIKE @SearchTerm OR Description LIKE @SearchTerm";
                var parameter = new SqlParameter("@SearchTerm", $"%{searchTerm}%");
                
                _logger.LogInformation("Searching dishes with term: {searchTerm}", searchTerm);
                var results = _dishRepository.ExecuteQuery(sql, parameter);
                
                _logger.LogInformation("Search found {count} dishes matching '{searchTerm}'", 
                    results.Count(), searchTerm);
                    
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching dishes with term: {searchTerm}", searchTerm);
                return Enumerable.Empty<Dish>();
            }
        }

        #region Helper Methods

        /// <summary>
        /// Validates and prepares a dish for saving by normalizing values
        /// </summary>
        private void ValidateAndPrepareDish(Dish dish)
        {
            // Basic validation
            if (dish == null)
                throw new ArgumentNullException(nameof(dish), "Dish cannot be null");
                
            if (string.IsNullOrEmpty(dish.Name))
                throw new ArgumentException("Dish name is required", nameof(dish));

            // Normalize the health factor value for consistency
            dish.HealthFactor = NormalizeHealthFactor(dish.HealthFactor);
                
            // Initialize collections to avoid null reference errors
            dish.Categories ??= new List<Category>();
            dish.Restaurants ??= new List<Restaurant>();
            dish.LikeDislikes ??= new List<LikeDislike>();
        }

        /// <summary>
        /// Standardizes health factor values for consistency
        /// </summary>
        private string NormalizeHealthFactor(string? healthFactor)
        {
            if (string.IsNullOrEmpty(healthFactor))
                return string.Empty;
                
            string normalized = healthFactor.ToLower().Trim(); // Fixed "trim()" to "Trim()"
            
            if (normalized.Contains("very healthy")) return "Very Healthy";
            if (normalized.Contains("healthy")) return "Healthy";
            if (normalized.Contains("moderate")) return "Moderate";
            if (normalized.Contains("less healthy")) return "Less Healthy";
            if (normalized.Contains("unhealthy")) return "Unhealthy";
            
            return healthFactor; // Keep original if no match
        }

        // Helper method to convert numeric health factor to display string
        private void ConvertHealthFactorForDisplay(Dish dish)
        {
            if (!string.IsNullOrEmpty(dish.HealthFactor) && int.TryParse(dish.HealthFactor, out int healthFactorValue))
            {
                // Store the original numeric value in a temporary variable
                string originalValue = dish.HealthFactor;
                
                // Convert to display text
                dish.HealthFactor = HealthFactorHelper.ToString(healthFactorValue);
                
                _logger.LogDebug("Converted health factor from {numeric} to {text} for dish {id}", 
                    originalValue, dish.HealthFactor, dish.DishID);
            }
        }

        // Generate unique dish ID
        private string GenerateUniqueId()
        {
            return $"d{DateTime.Now.Ticks % 900000 + 100000}";
        }

        #endregion
    }
}
