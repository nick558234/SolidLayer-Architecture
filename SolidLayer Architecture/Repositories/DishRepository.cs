using Microsoft.Data.SqlClient;
using SolidLayer_Architecture.Data;
using Swipe2TryCore.Models; // Use only this model namespace
using System.Data;

namespace SolidLayer_Architecture.Repositories
{
    /// <summary>
    /// Repository for managing dish data using direct SQL queries
    /// </summary>
    public class DishRepository : IRepository<Swipe2TryCore.Models.Dish> // Explicit namespace reference
    {
        private readonly string? _connectionString;
        private readonly ILogger<DishRepository> _logger;

        // Maximum field lengths to prevent database errors
        private const int MAX_ID_LENGTH = 10;
        private const int MAX_NAME_LENGTH = 100;
        private const int MAX_DESCRIPTION_LENGTH = 500;
        private const int MAX_PHOTO_LENGTH = 255;
        private const int MAX_HEALTHFACTOR_LENGTH = 20;

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

        #region CRUD Operations

        /// <summary>
        /// Retrieves all dishes from the database
        /// </summary>
        public IEnumerable<Swipe2TryCore.Models.Dish> GetAll()
        {
            return ExecuteQuery("SELECT * FROM DISHES");
        }

        /// <summary>
        /// Retrieves a specific dish by its ID
        /// </summary>
        public Swipe2TryCore.Models.Dish? GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("GetById called with null or empty ID");
                return null;
            }
            
            return ExecuteQuery("SELECT * FROM DISHES WHERE DishID = @DishID", 
                new SqlParameter("@DishID", id)).FirstOrDefault();
        }

        /// <summary>
        /// Inserts a new dish into the database
        /// </summary>
        public void Insert(Swipe2TryCore.Models.Dish dish)
        {
            try
            {
                // Ensure dish has a valid ID and all fields respect length constraints
                PrepareDishForStorage(dish);
                
                string sql = @"
                    INSERT INTO DISHES (DishID, Name, Description, Photo, HealthFactor) 
                    VALUES (@DishID, @Name, @Description, @Photo, @HealthFactor)";
                
                var parameters = CreateSqlParameters(dish);
                
                _logger.LogInformation("Inserting dish with ID {DishId}", dish.DishID);
                var rowsAffected = ExecuteNonQuery(sql, parameters);
                _logger.LogInformation("Insert successful, {Rows} rows affected", rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting dish. Error message: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing dish in the database
        /// </summary>
        public void Update(Swipe2TryCore.Models.Dish dish)
        {
            // Ensure fields respect length constraints
            string sql = @"
                UPDATE DISHES 
                SET Name = @Name, 
                    Description = @Description, 
                    Photo = @Photo, 
                    HealthFactor = @HealthFactor 
                WHERE DishID = @DishID";
            
            var parameters = CreateSqlParameters(dish);
            
            ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Deletes a dish from the database by ID
        /// </summary>
        public void Delete(string id)
        {
            ExecuteNonQuery("DELETE FROM DISHES WHERE DishID = @DishID", 
                new SqlParameter("@DishID", id));
        }

        #endregion

        #region SQL Execution Methods

        /// <summary>
        /// Executes a SQL query and maps the results to Dish objects
        /// </summary>
        public IEnumerable<Swipe2TryCore.Models.Dish> ExecuteQuery(string sql, params object[] parameters)
        {
            if (_connectionString == null)
            {
                _logger.LogError("Connection string is null");
                return Enumerable.Empty<Swipe2TryCore.Models.Dish>();
            }

            List<Swipe2TryCore.Models.Dish> dishes = new List<Swipe2TryCore.Models.Dish>();

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
                            dishes.Add(MapDishFromReader(reader));
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

        /// <summary>
        /// Executes a SQL command that doesn't return results (INSERT/UPDATE/DELETE)
        /// </summary>
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

        /// <summary>
        /// Executes a SQL command that returns a single result
        /// </summary>
        public Swipe2TryCore.Models.Dish? ExecuteScalar(string sql, params object[] parameters)
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates SQL parameters for a dish
        /// </summary>
        private SqlParameter[] CreateSqlParameters(Swipe2TryCore.Models.Dish dish)
        {
            return new[]
            {
                new SqlParameter("@DishID", dish.DishID),
                new SqlParameter("@Name", LimitLength(dish.Name, MAX_NAME_LENGTH)),
                new SqlParameter("@Description", LimitLength(dish.Description, MAX_DESCRIPTION_LENGTH)),
                new SqlParameter("@Photo", LimitLength(dish.Photo, MAX_PHOTO_LENGTH)),
                new SqlParameter("@HealthFactor", LimitLength(dish.HealthFactor, MAX_HEALTHFACTOR_LENGTH))
            };
        }

        /// <summary>
        /// Maps a data reader row to a Dish object
        /// </summary>
        private Swipe2TryCore.Models.Dish MapDishFromReader(SqlDataReader reader)
        {
            var dish = new Swipe2TryCore.Models.Dish
            {
                DishID = reader["DishID"].ToString() ?? string.Empty,
                Name = reader["Name"].ToString() ?? string.Empty,
                Description = reader["Description"]?.ToString() ?? string.Empty,
                Photo = reader["Photo"]?.ToString() ?? string.Empty,
                // Store health factor as string but it represents a numeric value
                HealthFactor = reader["HealthFactor"]?.ToString() ?? string.Empty,
                // Initialize collections to avoid null references
                Categories = new List<Category>(),
                Restaurants = new List<Restaurant>(),
                LikeDislikes = new List<LikeDislike>()
            };

            return dish;
        }

        /// <summary>
        /// Prepares a dish for database storage by ensuring all fields meet requirements
        /// </summary>
        private void PrepareDishForStorage(Swipe2TryCore.Models.Dish dish)
        {
            // Generate or clean up the dish ID
            dish.DishID = EnsureIdLength(dish.DishID ?? Guid.NewGuid().ToString());
            
            // Initialize collections
            dish.Categories ??= new List<Category>();
            dish.Restaurants ??= new List<Restaurant>();
            dish.LikeDislikes ??= new List<LikeDislike>();
        }

        /// <summary>
        /// Ensures ID is within allowed length and has valid format
        /// </summary>
        private string EnsureIdLength(string id, int maxLength = MAX_ID_LENGTH)
        {
            // If empty, generate a clean ID with 'd' prefix and numeric suffix
            if (string.IsNullOrEmpty(id))
            {
                return $"d{DateTime.Now.Ticks % 900000 + 100000}";
            }
                
            // If the ID is too long or contains hyphens, generate a new ID
            if (id.Length > maxLength || id.Contains("-"))
            {
                char firstChar = id.Length > 0 && char.IsLetter(id[0]) ? id[0] : 'd';
                return $"{firstChar}{new Random().Next(100000, 999999)}";
            }
            
            return id;
        }

        /// <summary>
        /// Ensures text fields don't exceed DB column length
        /// </summary>
        private string LimitLength(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
                
            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        #endregion
    }
}
