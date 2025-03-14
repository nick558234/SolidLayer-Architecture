using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace SolidLayer_Architecture.Pages
{
    public class DiagnosticsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiagnosticsModel> _logger;

        public DiagnosticsModel(IConfiguration configuration, ILogger<DiagnosticsModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
            DbConnectionError = string.Empty;
            MaskedConnectionString = string.Empty;
        }

        public bool DbConnectionSuccessful { get; private set; }
        public string DbConnectionError { get; private set; }
        public string MaskedConnectionString { get; private set; }
        public Dictionary<string, List<string>> TableStructure { get; private set; } = new Dictionary<string, List<string>>();

        public void OnGet()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            MaskedConnectionString = MaskConnectionString(connectionString ?? string.Empty);

            TestDatabaseConnection(connectionString ?? string.Empty);
            if (DbConnectionSuccessful)
            {
                GetTableStructure(connectionString ?? string.Empty);
            }
        }

        private void TestDatabaseConnection(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DbConnectionSuccessful = true;
                    _logger.LogInformation("Database connection test successful");
                }
            }
            catch (Exception ex)
            {
                DbConnectionSuccessful = false;
                DbConnectionError = ex.Message;
                _logger.LogError(ex, "Database connection test failed");
            }
        }

        private void GetTableStructure(string connectionString)
        {
            if (!DbConnectionSuccessful) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    // Check if the Dishes table exists
                    DataTable tables = connection.GetSchema("Tables");
                    
                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        
                        // Skip system tables
                        if (tableName.StartsWith("sys") || tableName.StartsWith("dt_"))
                            continue;
                        
                        List<string> columns = new List<string>();
                        
                        using (SqlCommand command = new SqlCommand($"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'", connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string columnName = reader["COLUMN_NAME"].ToString();
                                    string dataType = reader["DATA_TYPE"].ToString();
                                    columns.Add($"{columnName} ({dataType})");
                                }
                            }
                        }
                        
                        TableStructure.Add(tableName, columns);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving table structure");
            }
        }

        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "No connection string found";

            // Mask password if present
            var maskedString = Regex.Replace(connectionString, 
                @"(Password|pwd)=([^;]*)", 
                "Password=*****", 
                RegexOptions.IgnoreCase);
            
            return maskedString;
        }
    }
}
