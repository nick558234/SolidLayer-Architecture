using System.Data;
using Microsoft.Data.SqlClient;

namespace SolidLayer_Architecture.Helpers
{
    public static class QueryHelper
    {
        /// <summary>
        /// Executes a query and returns the results as a list of dictionaries
        /// </summary>
        public static List<Dictionary<string, object>> ExecuteQuery(
            string connectionString, string sql, ILogger logger)
        {
            var results = new List<Dictionary<string, object>>();
            
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();
                                
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row.Add(reader.GetName(i), reader[i] == DBNull.Value ? "NULL" : reader[i]);
                                }
                                
                                results.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing SQL query: {sql}", sql);
                throw;
            }
            
            return results;
        }
        
        /// <summary>
        /// Executes a non-query SQL statement and returns the number of rows affected
        /// </summary>
        public static int ExecuteNonQuery(string connectionString, string sql, ILogger logger)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing non-query SQL: {sql}", sql);
                throw;
            }
        }
    }
}
