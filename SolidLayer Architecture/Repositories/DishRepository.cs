using Microsoft.Data.SqlClient;
using SolidLayer_Architecture.Data;
using Swipe2TryCore.Models;
using System.Data;

namespace SolidLayer_Architecture.Repositories
{
    public class DishRepository : IRepository<Dish>
    {
        private readonly string? _connectionString;
        private readonly ILogger<DishRepository> _logger;

        public DishRepository(IConfiguration configuration, ILogger<DishRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("Connection string is null or empty");
                throw new InvalidOperationException("Connection string cannot be null or empty");
            }
        }

        // Helper method to ensure ID is within allowed length
        private string EnsureIdLength(string id, int maxLength = 10)
        {
            if (string.IsNullOrEmpty(id))
                return Guid.NewGuid().ToString().Substring(0, maxLength);
                
            return id.Length <= maxLength ? id : id.Substring(0, maxLength);
        }

        public IEnumerable<Dish> GetAll()
        {
            return ExecuteQuery("SELECT * FROM DISHES");
        }

        public Dish? GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("GetById called with null or empty ID");
                return null;
            }
            
            return ExecuteQuery("SELECT * FROM DISHES WHERE DishID = @DishID", 
                new SqlParameter("@DishID", id)).FirstOrDefault();
        }

        public void Insert(Dish dish)
        {
            // Ensure dish ID is not too long for the database column
            dish.DishID = EnsureIdLength(dish.DishID ?? Guid.NewGuid().ToString());
            
            string sql = @"
                INSERT INTO DISHES (DishID, Name, Description, Photo, HealthFactor) 
                VALUES (@DishID, @Name, @Description, @Photo, @HealthFactor)";
            
            var parameters = new[]
            {
                new SqlParameter("@DishID", dish.DishID),
                new SqlParameter("@Name", dish.Name?.Length > 100 ? dish.Name.Substring(0, 100) : dish.Name ?? string.Empty),
                new SqlParameter("@Description", dish.Description?.Length > 500 ? dish.Description.Substring(0, 500) : dish.Description ?? string.Empty),
                new SqlParameter("@Photo", dish.Photo?.Length > 255 ? dish.Photo.Substring(0, 255) : dish.Photo ?? string.Empty),
                new SqlParameter("@HealthFactor", dish.HealthFactor?.Length > 50 ? dish.HealthFactor.Substring(0, 50) : dish.HealthFactor ?? string.Empty)
            };
            
            _logger.LogInformation("Inserting dish with ID {DishId} using raw SQL", dish.DishID);
            var rowsAffected = ExecuteNonQuery(sql, parameters);
            _logger.LogInformation("Insert successful, {Rows} rows affected", rowsAffected);
        }

        public void Update(Dish dish)
        {
            string sql = "UPDATE DISHES SET Name = @Name, Description = @Description, Photo = @Photo, HealthFactor = @HealthFactor WHERE DishID = @DishID";
            
            var parameters = new[]
            {
                new SqlParameter("@DishID", dish.DishID),
                new SqlParameter("@Name", dish.Name?.Length > 100 ? dish.Name.Substring(0, 100) : dish.Name ?? string.Empty),
                new SqlParameter("@Description", dish.Description?.Length > 500 ? dish.Description.Substring(0, 500) : dish.Description ?? string.Empty),
                new SqlParameter("@Photo", dish.Photo?.Length > 255 ? dish.Photo.Substring(0, 255) : dish.Photo ?? string.Empty),
                new SqlParameter("@HealthFactor", dish.HealthFactor?.Length > 50 ? dish.HealthFactor.Substring(0, 50) : dish.HealthFactor ?? string.Empty)
            };
            
            ExecuteNonQuery(sql, parameters);
        }

        public void Delete(string id)
        {
            ExecuteNonQuery("DELETE FROM DISHES WHERE DishID = @DishID", new SqlParameter("@DishID", id));
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
                        return GetById(result.ToString() ?? "");
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
