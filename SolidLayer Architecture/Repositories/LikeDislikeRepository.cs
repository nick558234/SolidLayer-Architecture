using Microsoft.Data.SqlClient;
using Swipe2TryCore.Models; // Use only this model namespace
using System.Data;

namespace SolidLayer_Architecture.Repositories
{
    public class LikeDislikeRepository : IRepository<Swipe2TryCore.Models.LikeDislike>
    {
        private readonly string? _connectionString;
        private readonly ILogger<LikeDislikeRepository> _logger;

        public LikeDislikeRepository(IConfiguration configuration, ILogger<LikeDislikeRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("Connection string is null or empty");
                throw new InvalidOperationException("Connection string cannot be null or empty");
            }

            // Ensure the LIKES_DISLIKES table exists
            EnsureTableExists();
        }

        private void EnsureTableExists()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    // Check if LIKES_DISLIKES table exists
                    bool tableExists = false;
                    using (SqlCommand command = new SqlCommand(
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LIKES_DISLIKES'", connection))
                    {
                        int tableCount = (int)command.ExecuteScalar();
                        tableExists = tableCount > 0;
                    }
                    
                    if (!tableExists)
                    {
                        // Create the LIKES_DISLIKES table
                        _logger.LogInformation("Creating LIKES_DISLIKES table");
                        string createTableSql = @"
                            CREATE TABLE LIKES_DISLIKES (
                                LikeDislikeID NVARCHAR(10) PRIMARY KEY,
                                UserID NVARCHAR(10) NOT NULL,
                                DishID NVARCHAR(10) NOT NULL,
                                IsLike BIT NOT NULL,
                                CreatedAt DATETIME DEFAULT GETDATE()
                            )";
                            
                        using (SqlCommand command = new SqlCommand(createTableSql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring LIKES_DISLIKES table exists");
            }
        }

        public IEnumerable<Swipe2TryCore.Models.LikeDislike> GetAll()
        {
            return ExecuteQuery("SELECT * FROM LIKES_DISLIKES");
        }

        public Swipe2TryCore.Models.LikeDislike? GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("GetById called with null or empty ID");
                return null;
            }
            
            return ExecuteQuery("SELECT * FROM LIKES_DISLIKES WHERE LikeDislikeID = @LikeDislikeID", 
                new SqlParameter("@LikeDislikeID", id)).FirstOrDefault();
        }

        public void Insert(Swipe2TryCore.Models.LikeDislike likeDislike)
        {
            // Generate a new ID if none is provided
            if (string.IsNullOrEmpty(likeDislike.LikeDislikeID))
            {
                likeDislike.LikeDislikeID = Guid.NewGuid().ToString().Substring(0, 10);
            }
            
            string sql = @"
                INSERT INTO LIKES_DISLIKES (LikeDislikeID, UserID, DishID, IsLike) 
                VALUES (@LikeDislikeID, @UserID, @DishID, @IsLike)";
            
            var parameters = new[]
            {
                new SqlParameter("@LikeDislikeID", likeDislike.LikeDislikeID),
                new SqlParameter("@UserID", likeDislike.UserID),
                new SqlParameter("@DishID", likeDislike.DishID),
                new SqlParameter("@IsLike", likeDislike.IsLike)
            };
            
            _logger.LogInformation("Inserting like/dislike with ID {Id}", likeDislike.LikeDislikeID);
            var rowsAffected = ExecuteNonQuery(sql, parameters);
            _logger.LogInformation("Insert successful, {Rows} rows affected", rowsAffected);
        }

        public void Update(Swipe2TryCore.Models.LikeDislike likeDislike)
        {
            string sql = "UPDATE LIKES_DISLIKES SET UserID = @UserID, DishID = @DishID, IsLike = @IsLike WHERE LikeDislikeID = @LikeDislikeID";
            
            var parameters = new[]
            {
                new SqlParameter("@LikeDislikeID", likeDislike.LikeDislikeID),
                new SqlParameter("@UserID", likeDislike.UserID),
                new SqlParameter("@DishID", likeDislike.DishID),
                new SqlParameter("@IsLike", likeDislike.IsLike)
            };
            
            ExecuteNonQuery(sql, parameters);
        }

        public void Delete(string id)
        {
            ExecuteNonQuery("DELETE FROM LIKES_DISLIKES WHERE LikeDislikeID = @LikeDislikeID", 
                new SqlParameter("@LikeDislikeID", id));
        }

        public IEnumerable<Swipe2TryCore.Models.LikeDislike> GetByUserIdAndDishId(string userId, string dishId)
        {
            string sql = "SELECT * FROM LIKES_DISLIKES WHERE UserID = @UserID AND DishID = @DishID";
            return ExecuteQuery(sql, new SqlParameter("@UserID", userId), new SqlParameter("@DishID", dishId));
        }

        public IEnumerable<Swipe2TryCore.Models.LikeDislike> GetByUserId(string userId)
        {
            string sql = "SELECT * FROM LIKES_DISLIKES WHERE UserID = @UserID";
            return ExecuteQuery(sql, new SqlParameter("@UserID", userId));
        }

        public IEnumerable<Swipe2TryCore.Models.LikeDislike> GetByDishId(string dishId)
        {
            string sql = "SELECT * FROM LIKES_DISLIKES WHERE DishID = @DishID";
            return ExecuteQuery(sql, new SqlParameter("@DishID", dishId));
        }

        public int GetLikeCount(string dishId)
        {
            string sql = "SELECT COUNT(*) FROM LIKES_DISLIKES WHERE DishID = @DishID AND IsLike = 1";
            
            try
            {
                if (_connectionString == null)
                {
                    _logger.LogError("Connection string is null");
                    return 0;
                }
                
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@DishID", dishId));
                        return (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting like count for dish: {DishId}", dishId);
                return 0;
            }
        }

        public int GetDislikeCount(string dishId)
        {
            string sql = "SELECT COUNT(*) FROM LIKES_DISLIKES WHERE DishID = @DishID AND IsLike = 0";
            
            try
            {
                if (_connectionString == null)
                {
                    _logger.LogError("Connection string is null");
                    return 0;
                }
                
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@DishID", dishId));
                        return (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dislike count for dish: {DishId}", dishId);
                return 0;
            }
        }

        public IEnumerable<Swipe2TryCore.Models.LikeDislike> ExecuteQuery(string sql, params object[] parameters)
        {
            if (_connectionString == null)
            {
                _logger.LogError("Connection string is null");
                return Enumerable.Empty<Swipe2TryCore.Models.LikeDislike>();
            }

            List<Swipe2TryCore.Models.LikeDislike> likeDislikes = new List<Swipe2TryCore.Models.LikeDislike>();

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
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                // Create and initialize a new LikeDislike with data from the reader
                                var likeDislike = new Swipe2TryCore.Models.LikeDislike
                                {
                                    LikeDislikeID = reader["LikeDislikeID"]?.ToString() ?? string.Empty,
                                    UserID = reader["UserID"]?.ToString() ?? string.Empty,
                                    DishID = reader["DishID"]?.ToString() ?? string.Empty,
                                    IsLike = Convert.ToBoolean(reader["IsLike"]),
                                    // Navigation properties will be null initially
                                    User = null!, 
                                    Dish = null!
                                };
                                
                                likeDislikes.Add(likeDislike);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error mapping SQL result to LikeDislike object");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query: {Sql}", sql);
                throw;
            }

            return likeDislikes;
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
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing non-query: {Sql}", sql);
                throw;
            }
        }

        public Swipe2TryCore.Models.LikeDislike? ExecuteScalar(string sql, params object[] parameters)
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
                    
                    if (result != null && result != DBNull.Value)
                    {
                        string id = result.ToString() ?? string.Empty;
                        return GetById(id); // Safe to pass non-null string
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
