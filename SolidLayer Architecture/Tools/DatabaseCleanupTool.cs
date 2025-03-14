using Microsoft.Data.SqlClient;
using System;
using System.Data;  // Add this using directive for DataTable and DataRow

namespace SolidLayer_Architecture.Tools
{
    public class DatabaseCleanupTool
    {
        private readonly string? _connectionString;
        private readonly ILogger<DatabaseCleanupTool> _logger;

        public DatabaseCleanupTool(IConfiguration configuration, ILogger<DatabaseCleanupTool> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public void CleanupDishIds()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("Connection string is null or empty");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Starting dish ID cleanup");

                    // 1. Backup all dish data to a temporary table
                    CreateBackupTable(connection);
                    
                    // 2. Fix any potentially invalid IDs
                    NormalizeDishIds(connection);

                    _logger.LogInformation("Dish ID cleanup completed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during dish ID cleanup");
            }
        }

        private void CreateBackupTable(SqlConnection connection)
        {
            _logger.LogInformation("Creating backup of dishes table");
            
            try
            {
                // 1. Drop backup table if exists
                string dropBackupSql = @"
                    IF OBJECT_ID('DISHES_BACKUP', 'U') IS NOT NULL 
                        DROP TABLE DISHES_BACKUP";
                
                using (SqlCommand command = new SqlCommand(dropBackupSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                // 2. Create backup table
                string createBackupSql = @"
                    SELECT * 
                    INTO DISHES_BACKUP 
                    FROM DISHES";
                
                using (SqlCommand command = new SqlCommand(createBackupSql, connection))
                {
                    command.ExecuteNonQuery();
                    _logger.LogInformation("Backup table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup table");
                throw;
            }
        }

        private void NormalizeDishIds(SqlConnection connection)
        {
            _logger.LogInformation("Normalizing dish IDs");
            
            try
            {
                // 1. Get all dishes with problematic IDs
                DataTable dishes = new DataTable();
                string selectDishesQuery = @"
                    SELECT DishID, Name 
                    FROM DISHES 
                    WHERE LEN(DishID) > 10 OR DishID LIKE '%-%' OR ISNUMERIC(DishID) = 0";
                
                using (SqlCommand command = new SqlCommand(selectDishesQuery, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    dishes.Load(reader);
                }

                if (dishes.Rows.Count == 0)
                {
                    _logger.LogInformation("No dishes with problematic IDs found");
                    return;
                }

                _logger.LogInformation("Found {count} dishes with problematic IDs", dishes.Rows.Count);

                // 2. Fix each dish ID
                foreach (DataRow dishRow in dishes.Rows)
                {
                    string oldId = dishRow["DishID"].ToString() ?? "";
                    string dishName = dishRow["Name"].ToString() ?? "Unknown";
                    
                    // Generate a new clean ID (simple sequential numbers)
                    string newId = Guid.NewGuid().ToString().Substring(0, 8);
                    
                    // Update the dish ID
                    string updateDishSql = @"
                        UPDATE DISHES
                        SET DishID = @NewId
                        WHERE DishID = @OldId";
                    
                    using (SqlCommand command = new SqlCommand(updateDishSql, connection))
                    {
                        command.Parameters.AddWithValue("@NewId", newId);
                        command.Parameters.AddWithValue("@OldId", oldId);
                        
                        int rowsAffected = command.ExecuteNonQuery();
                        _logger.LogInformation("Updated dish ID from {oldId} to {newId} for dish: {dishName}", 
                            oldId, newId, dishName);
                    }

                    // Update any associated likes/dislikes
                    string updateLikeDislikeSql = @"
                        UPDATE LIKES_DISLIKES
                        SET DishID = @NewId
                        WHERE DishID = @OldId";
                    
                    using (SqlCommand command = new SqlCommand(updateLikeDislikeSql, connection))
                    {
                        command.Parameters.AddWithValue("@NewId", newId);
                        command.Parameters.AddWithValue("@OldId", oldId);
                        
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            _logger.LogInformation("Updated {count} like/dislike references for dish ID: {oldId}", 
                                rowsAffected, oldId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error normalizing dish IDs");
                throw;
            }
        }
    }
}
