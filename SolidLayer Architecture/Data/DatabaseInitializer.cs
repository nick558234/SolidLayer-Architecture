using Microsoft.Data.SqlClient;
using System.Data;

namespace SolidLayer_Architecture.Data
{
    public class DatabaseInitializer
    {
        private readonly string? _connectionString;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public void EnsureDatabaseExists()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("Connection string is null or empty");
                return;
            }

            try
            {
                // Parse the database name from connection string
                var builder = new SqlConnectionStringBuilder(_connectionString);
                string databaseName = builder.InitialCatalog;
                
                // Create a connection string to the master database
                builder.InitialCatalog = "master";
                string masterConnectionString = builder.ConnectionString;

                using (SqlConnection connection = new SqlConnection(masterConnectionString))
                {
                    connection.Open();
                    
                    // Check if database exists
                    using (SqlCommand command = new SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'", connection))
                    {
                        int dbCount = (int)command.ExecuteScalar();
                        
                        if (dbCount == 0)
                        {
                            // Create the database
                            _logger.LogInformation("Creating database {DatabaseName}", databaseName);
                            using (SqlCommand createCommand = new SqlCommand($"CREATE DATABASE [{databaseName}]", connection))
                            {
                                createCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // Now connect to the database to ensure tables exist
                EnsureTablesExist(_connectionString);
                
                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring database exists");
            }
        }

        private void EnsureTablesExist(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    // Check and create DISHES table
                    EnsureDishesTableExists(connection);
                    
                    // Check and create LIKES_DISLIKES table
                    EnsureLikesDislikesTableExists(connection);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring tables exist");
            }
        }

        private void EnsureDishesTableExists(SqlConnection connection)
        {
            bool tableExists = TableExists(connection, "DISHES");
            
            if (!tableExists)
            {
                // Create the DISHES table
                _logger.LogInformation("Creating DISHES table");
                string createTableSql = @"
                    CREATE TABLE DISHES (
                        DishID NVARCHAR(10) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Description NVARCHAR(500) NULL,
                        Photo NVARCHAR(255) NULL,
                        HealthFactor NVARCHAR(20) NULL
                    )";
                    
                using (SqlCommand command = new SqlCommand(createTableSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                // If table exists but HealthFactor column is too small, we should alter it
                // Using INFORMATION_SCHEMA to check column length
                string checkColumnSql = @"
                    SELECT CHARACTER_MAXIMUM_LENGTH 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'DISHES' AND COLUMN_NAME = 'HealthFactor'";
                
                int columnLength = 0;
                using (SqlCommand command = new SqlCommand(checkColumnSql, connection))
                {
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        columnLength = Convert.ToInt32(result);
                    }
                }
                
                // If column is smaller than 20 characters, alter it
                if (columnLength > 0 && columnLength < 20)
                {
                    _logger.LogInformation("Altering DISHES.HealthFactor column to NVARCHAR(20)");
                    string alterColumnSql = @"ALTER TABLE DISHES ALTER COLUMN HealthFactor NVARCHAR(20) NULL";
                    
                    using (SqlCommand command = new SqlCommand(alterColumnSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void EnsureLikesDislikesTableExists(SqlConnection connection)
        {
            bool tableExists = TableExists(connection, "LIKES_DISLIKES");
            
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

        private bool TableExists(SqlConnection connection, string tableName)
        {
            using (SqlCommand command = new SqlCommand(
                $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'", connection))
            {
                int tableCount = (int)command.ExecuteScalar();
                return tableCount > 0;
            }
        }
    }
}
