using Microsoft.Data.SqlClient;
using SolidLayer_Architecture.Data;
using SolidLayer_Architecture.Interfaces.Repositories;
using Swipe2TryCore.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SolidLayer_Architecture.Repositories
{
    public class DishRepository : IDishRepository
    {
        private readonly DatabaseInitializer _dbInitializer;

        public DishRepository(DatabaseInitializer dbInitializer)
        {
            _dbInitializer = dbInitializer;
        }

        public async Task<IEnumerable<Dish>> GetAllDishesAsync()
        {
            var dishes = new List<Dish>();

            using (var connection = _dbInitializer.CreateConnection())
            {
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Dishes";
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    dishes.Add(MapDishFromReader(reader));
                }
            }
            
            return dishes;
        }

        public async Task<Dish> GetDishByIdAsync(string id)
        {
            using (var connection = _dbInitializer.CreateConnection())
            {
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Dishes WHERE DishID = @DishID";
                command.Parameters.Add(new SqlParameter("@DishID", SqlDbType.NVarChar) { Value = id });
                
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapDishFromReader(reader);
                }
                
                return null;
            }
        }

        public async Task<Dish> CreateDishAsync(Dish dish)
        {
            using (var connection = _dbInitializer.CreateConnection())
            {
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Dishes (DishID, Name, Description, Photo, HealthFactor)
                    VALUES (@DishID, @Name, @Description, @Photo, @HealthFactor)";
                
                // If DishID is null or empty, generate a new GUID
                if (string.IsNullOrEmpty(dish.DishID))
                {
                    dish.DishID = Guid.NewGuid().ToString();
                }
                
                AddDishParameters(command, dish);
                
                await command.ExecuteNonQueryAsync();
                
                return dish;
            }
        }

        public async Task<bool> UpdateDishAsync(Dish dish)
        {
            using (var connection = _dbInitializer.CreateConnection())
            {
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Dishes 
                    SET Name = @Name,
                        Description = @Description,
                        Photo = @Photo,
                        HealthFactor = @HealthFactor
                    WHERE DishID = @DishID";
                
                AddDishParameters(command, dish);
                
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteDishAsync(string id)
        {
            using (var connection = _dbInitializer.CreateConnection())
            {
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Dishes WHERE DishID = @DishID";
                command.Parameters.Add(new SqlParameter("@DishID", SqlDbType.NVarChar) { Value = id });
                
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        private Dish MapDishFromReader(SqlDataReader reader)
        {
            return new Dish
            {
                DishID = reader["DishID"].ToString(),
                Name = reader["Name"].ToString(),
                Description = reader["Description"]?.ToString(),
                Photo = reader["Photo"]?.ToString(),
                HealthFactor = reader["HealthFactor"]?.ToString()
                // Note: Not loading related collections here
            };
        }

        private void AddDishParameters(SqlCommand command, Dish dish)
        {
            command.Parameters.Add(new SqlParameter("@DishID", SqlDbType.NVarChar) { Value = dish.DishID });
            command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar) { Value = dish.Name });
            command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar) { 
                Value = dish.Description ?? (object)DBNull.Value 
            });
            command.Parameters.Add(new SqlParameter("@Photo", SqlDbType.NVarChar) { 
                Value = dish.Photo ?? (object)DBNull.Value 
            });
            command.Parameters.Add(new SqlParameter("@HealthFactor", SqlDbType.NVarChar) { 
                Value = dish.HealthFactor ?? (object)DBNull.Value 
            });
        }
    }
}