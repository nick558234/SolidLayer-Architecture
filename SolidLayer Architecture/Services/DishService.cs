using Microsoft.Data.SqlClient;
using SolidLayer_Architecture.Repositories;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Services
{
    public class DishService : IDishService
    {
        private readonly IRepository<Dish> _dishRepository;
        private readonly ILogger<DishService> _logger;

        public DishService(IRepository<Dish> dishRepository, ILogger<DishService> logger)
        {
            _dishRepository = dishRepository;
            _logger = logger;
        }

        public IEnumerable<Dish> GetAllDishes()
        {
            try
            {
                return _dishRepository.GetAll();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all dishes");
                return Enumerable.Empty<Dish>();
            }
        }

        public Dish? GetDishById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("GetDishById called with null or empty ID");
                return null;
            }
            
            try
            {
                return _dishRepository.GetById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dish by ID: {DishId}", id);
                return null;
            }
        }

        // Helper method to ensure ID is within allowed length
        private string EnsureIdLength(string id, int maxLength = 10)
        {
            if (string.IsNullOrEmpty(id))
                return Guid.NewGuid().ToString().Substring(0, maxLength);
                
            return id.Length <= maxLength ? id : id.Substring(0, maxLength);
        }

        public void AddDish(Dish dish)
        {
            try
            {
                if (dish.DishID == null)
                {
                    dish.DishID = EnsureIdLength(Guid.NewGuid().ToString());
                }
                else
                {
                    dish.DishID = EnsureIdLength(dish.DishID);
                }

                // Initialize collections to avoid null reference errors
                dish.Categories ??= new List<Category>();
                dish.Restaurants ??= new List<Restaurant>();
                dish.LikeDislikes ??= new List<LikeDislike>();
                
                _logger.LogInformation("Adding dish: {DishId} - {Name}", dish.DishID, dish.Name);
                _dishRepository.Insert(dish);
                _logger.LogInformation("Dish added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding dish: {DishId} - {Name}", dish.DishID, dish.Name);
                throw;
            }
        }

        public void UpdateDish(Dish dish)
        {
            try
            {
                _dishRepository.Update(dish);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish: {DishId}", dish.DishID);
                throw;
            }
        }

        public void DeleteDish(string id)
        {
            try
            {
                _dishRepository.Delete(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dish: {DishId}", id);
                throw;
            }
        }

        public IEnumerable<Dish> SearchDishes(string searchTerm)
        {
            try
            {
                string sql = "SELECT * FROM DISHES WHERE Name LIKE @SearchTerm OR Description LIKE @SearchTerm";
                var parameter = new SqlParameter("@SearchTerm", $"%{searchTerm}%");
                return _dishRepository.ExecuteQuery(sql, parameter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching dishes with term: {SearchTerm}", searchTerm);
                return Enumerable.Empty<Dish>();
            }
        }
    }
}
