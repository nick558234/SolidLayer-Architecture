using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SolidLayer_Architecture.Data;
using Swipe2TryCore.Models;
using System.Data;

namespace SolidLayer_Architecture.Repositories
{
    public class DishRepository : IRepository<Dish>
    {
        private readonly ApplicationDbContext _context;
        private readonly string? _connectionString;
        private readonly ILogger<DishRepository> _logger;

        public DishRepository(ApplicationDbContext context, IConfiguration configuration, ILogger<DishRepository> logger)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("Connection string is null or empty");
                throw new InvalidOperationException("Connection string cannot be null or empty");
            }
        }

        public IEnumerable<Dish> GetAll()
        {
            try
            {
                // Using raw SQL instead of EF Core with uppercase table name
                return ExecuteQuery("SELECT * FROM DISHES");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all dishes: {Message}", ex.Message);
                // Fallback to EF Core if SQL fails
                return _context.Dishes.ToList();
            }
        }

        public Dish? GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("GetById called with null or empty ID");
                return null;
            }
            
            try
            {
                var dishes = ExecuteQuery("SELECT * FROM DISHES WHERE DishID = @DishID", new SqlParameter("@DishID", id));
                return dishes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dish by ID: {DishId}. Error message: {Message}", id, ex.Message);
                // Fallback to EF Core
                return _context.Dishes.FirstOrDefault(d => d.DishID == id);
            }
        }

        // Helper method to ensure ID is within allowed length
        private string EnsureIdLength(string id, int maxLength = 10)
        {
            if (string.IsNullOrEmpty(id))
                return Guid.NewGuid().ToString().Substring(0, maxLength);
                
            return id.Length <= maxLength ? id : id.Substring(0, maxLength);
        }

        public void Insert(Dish dish)
        {
            try
            {
                // Ensure dish ID is not too long for the database column
                dish.DishID = EnsureIdLength(dish.DishID);
                
                // First try with ExecuteNonQuery - adjust table and column names to match actual DB schema
                string sql = @"
                    INSERT INTO DISHES (DishID, Name, Description, Photo, HealthFactor) 
                    VALUES (@DishID, @Name, @Description, @Photo, @HealthFactor)";
                
                var dishId = EnsureIdLength(dish.DishID ?? Guid.NewGuid().ToString());
                
                var parameters = new[]
                {
                    new SqlParameter("@DishID", dishId),
                    new SqlParameter("@Name", dish.Name?.Length > 100 ? dish.Name.Substring(0, 100) : dish.Name ?? string.Empty),
                    new SqlParameter("@Description", dish.Description?.Length > 500 ? dish.Description.Substring(0, 500) : dish.Description ?? string.Empty),
                    new SqlParameter("@Photo", dish.Photo?.Length > 255 ? dish.Photo.Substring(0, 255) : dish.Photo ?? string.Empty),
                    new SqlParameter("@HealthFactor", dish.HealthFactor?.Length > 50 ? dish.HealthFactor.Substring(0, 50) : dish.HealthFactor ?? string.Empty)
                };
                
                _logger.LogInformation("Attempting to insert dish with ID {DishId} using raw SQL", dishId);
                var rowsAffected = ExecuteNonQuery(sql, parameters);
                _logger.LogInformation("Insert successful, {Rows} rows affected", rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting dish with raw SQL. Error message: {Message}", ex.Message);
                
                // Fallback to EF Core
                try
                {
                    // Ensure dish ID is not too long for the database column
                    if (string.IsNullOrEmpty(dish.DishID))
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
                    
                    _logger.LogInformation("Falling back to EF Core for insertion. Dish ID: {DishId}", dish.DishID);
                    _context.Dishes.Add(dish);
                    _context.SaveChanges();
                    _logger.LogInformation("EF Core insertion successful");
                }
                catch (Exception efEx)
                {
                    _logger.LogError(efEx, "EF Core fallback also failed when inserting dish. Error message: {Message}", efEx.Message);
                    throw; // Re-throw if both methods fail
                }
            }
        }

        public void Update(Dish dish)
        {
            try
            {
                string sql = "UPDATE DISHES SET Name = @Name, Description = @Description, Photo = @Photo, HealthFactor = @HealthFactor WHERE DishID = @DishID";
                
                var parameters = new[]
                {
                    new SqlParameter("@DishID", dish.DishID),
                    new SqlParameter("@Name", dish.Name ?? string.Empty),
                    new SqlParameter("@Description", dish.Description ?? string.Empty),
                    new SqlParameter("@Photo", dish.Photo ?? string.Empty),
                    new SqlParameter("@HealthFactor", dish.HealthFactor ?? string.Empty)
                };
                
                ExecuteNonQuery(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish with raw SQL. Falling back to EF Core");
                
                // Fallback to EF Core
                try
                {
                    _context.Dishes.Update(dish);
                    _context.SaveChanges();
                }
                catch (Exception efEx)
                {
                    _logger.LogError(efEx, "EF Core fallback also failed when updating dish");
                    throw;
                }
            }
        }

        public void Delete(string id)
        {
            try
            {
                ExecuteNonQuery("DELETE FROM DISHES WHERE DishID = @DishID", new SqlParameter("@DishID", id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dish with raw SQL. Falling back to EF Core");
                
                // Fallback to EF Core
                try
                {
                    var dish = _context.Dishes.Find(id);
                    if (dish != null)
                    {
                        _context.Dishes.Remove(dish);
                        _context.SaveChanges();
                    }
                }
                catch (Exception efEx)
                {
                    _logger.LogError(efEx, "EF Core fallback also failed when deleting dish");
                    throw;
                }
            }
        }

        public IEnumerable<Dish> ExecuteQuery(string sql, params object[] parameters)
        {
            if (_connectionString == null)
            {
                _logger.LogError("Connection string is null");
                return Enumerable.Empty<Dish>();
            }

            List<Dish> dishes = new List<Dish>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand(sql, connection);
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    _logger.LogInformation("Executing SQL query: {Sql}", sql);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var dish = new Dish
                                {
                                    DishID = reader["DishID"]?.ToString() ?? string.Empty,
                                    Name = reader["Name"]?.ToString() ?? string.Empty,
                                    Description = reader["Description"]?.ToString() ?? string.Empty,
                                    Photo = reader["Photo"]?.ToString() ?? string.Empty,
                                    HealthFactor = reader["HealthFactor"]?.ToString() ?? string.Empty,
                                    Categories = new List<Category>(),
                                    Restaurants = new List<Restaurant>(),
                                    LikeDislikes = new List<LikeDislike>()
                                };
                                dishes.Add(dish);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error mapping SQL result to Dish object. Column may be missing.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query: {Sql}. Error message: {Message}", sql, ex.Message);
                throw;
            }

            return dishes;
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            if (_connectionString == null)
            {
                _logger.LogError("Connection string is null");
                return 0;
            }
            
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand(sql, connection);
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    _logger.LogInformation("Executing SQL non-query: {Sql}", sql);
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing non-query: {Sql}", sql);
                throw;
            }
        }

        public Dish? ExecuteScalar(string sql, params object[] parameters)
        {
            if (_connectionString == null)
            {
                _logger.LogError("Connection string is null");
                return null;
            }
            
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand(sql, connection);
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    _logger.LogInformation("Executing SQL scalar: {Sql}", sql);
                    var result = command.ExecuteScalar();
                    
                    if (result != null)
                    {
                        return GetById(result.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scalar: {Sql}", sql);
                throw;
            }
            
            return null;
        }
    }
}
