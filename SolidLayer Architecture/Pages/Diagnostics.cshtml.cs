using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using SolidLayer_Architecture.Tools;

namespace SolidLayer_Architecture.Pages
{
    public class DiagnosticsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseCleanupTool _cleanupTool;
        private readonly ILogger<DiagnosticsModel> _logger;

        public DiagnosticsModel(
            IConfiguration configuration, 
            DatabaseCleanupTool cleanupTool,
            ILogger<DiagnosticsModel> logger)
        {
            _configuration = configuration;
            _cleanupTool = cleanupTool;
            _logger = logger;
            DbConnectionError = string.Empty;
            MaskedConnectionString = string.Empty;
        }

        public bool DbConnectionSuccessful { get; private set; }
        public string DbConnectionError { get; private set; }
        public string MaskedConnectionString { get; private set; }
        public Dictionary<string, List<string>> TableStructure { get; private set; } = new Dictionary<string, List<string>>();
        public bool DatabaseFixed { get; private set; }
        public bool DishIdsFixed { get; private set; }
        
        [BindProperty]
        public bool RunDatabaseFix { get; set; }
        
        [BindProperty]
        public bool RunDishIdFix { get; set; }

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

        public IActionResult OnPost()
        {
            if (RunDatabaseFix)
            {
                FixDatabase(_configuration.GetConnectionString("DefaultConnection") ?? string.Empty);
            }

            if (RunDishIdFix)
            {
                _cleanupTool.CleanupDishIds();
                DishIdsFixed = true;
            }
            
            // Reload the page data
            OnGet();
            return Page();
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

        private void FixDatabase(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    // Update the HealthFactor column size
                    string alterTable = "ALTER TABLE DISHES ALTER COLUMN HealthFactor NVARCHAR(20) NULL";
                    using (SqlCommand command = new SqlCommand(alterTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    
                    // Truncate any values that are too long
                    string updateData = @"
                        UPDATE DISHES 
                        SET HealthFactor = LEFT(HealthFactor, 20)
                        WHERE LEN(HealthFactor) > 20";
                    
                    using (SqlCommand command = new SqlCommand(updateData, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    
                    DatabaseFixed = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing database: {Message}", ex.Message);
                DatabaseFixed = false;
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
