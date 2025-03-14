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
                    
                    // Check if DISHES table exists
                    bool dishesExists = false;
                    using (SqlCommand command = new SqlCommand(
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DISHES'", connection))
                    {
                        int tableCount = (int)command.ExecuteScalar();
                        dishesExists = tableCount > 0;
                    }
                    
                    if (!dishesExists)
                    {
                        // Create the DISHES table
                        _logger.LogInformation("Creating DISHES table");
                        string createTableSql = @"
                            CREATE TABLE DISHES (
                                DishID NVARCHAR(10) PRIMARY KEY,
                                Name NVARCHAR(100) NOT NULL,
                                Description NVARCHAR(500) NULL,
                                Photo NVARCHAR(255) NULL,
                                HealthFactor NVARCHAR(50) NULL
                            )";
                            
                        using (SqlCommand command = new SqlCommand(createTableSql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    // Add other tables as needed...
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring tables exist");
            }
        }
    }
}
