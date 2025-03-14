using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SolidLayer_Architecture.Pages
{
    public class ManualQueryModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ManualQueryModel> _logger;

        public ManualQueryModel(IConfiguration configuration, ILogger<ManualQueryModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public string SqlQuery { get; set; }

        [BindProperty]
        public string QueryType { get; set; } = "Select";

        public DataTable Results { get; private set; }

        public int? AffectedRows { get; private set; }

        public string ErrorMessage { get; private set; }

        public void OnGet()
        {
            // Default query example
            SqlQuery = "SELECT * FROM DISHES";
        }

        public IActionResult OnPost()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(SqlQuery))
                {
                    ErrorMessage = "SQL query cannot be empty.";
                    return Page();
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(SqlQuery, connection);

                    if (QueryType == "Select")
                    {
                        // Execute SELECT query
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            Results = new DataTable();
                            adapter.Fill(Results);
                        }
                        
                        _logger.LogInformation("SELECT query executed successfully. Rows returned: {RowCount}", 
                            Results?.Rows.Count ?? 0);
                    }
                    else
                    {
                        // Execute non-query (INSERT, UPDATE, DELETE)
                        AffectedRows = command.ExecuteNonQuery();
                        _logger.LogInformation("Non-query executed successfully. Rows affected: {RowCount}", AffectedRows);
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error executing SQL query: {Query}", SqlQuery);
                return Page();
            }
        }
    }
}
