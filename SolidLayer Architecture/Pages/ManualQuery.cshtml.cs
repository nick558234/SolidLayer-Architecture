using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Helpers;

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
        public string SqlQuery { get; set; } = "SELECT * FROM DISHES";
        
        public List<Dictionary<string, object>> QueryResults { get; set; } = new List<Dictionary<string, object>>();
        
        public string ErrorMessage { get; set; } = string.Empty;
        
        public bool IsReadOnly { get; set; } = true;
        
        public void OnGet()
        {
            // Initialize page with default query
        }
        
        public IActionResult OnPost(bool executeAsReadOnly = true)
        {
            IsReadOnly = executeAsReadOnly;
            
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                    throw new InvalidOperationException("Connection string not found.");
                
                if (IsReadOnly)
                {
                    // Execute as read-only query
                    QueryResults = QueryHelper.ExecuteQuery(connectionString, SqlQuery, _logger);
                }
                else
                {
                    // Execute as potentially modifying query
                    int rowsAffected = QueryHelper.ExecuteNonQuery(connectionString, SqlQuery, _logger);
                    Dictionary<string, object> resultRow = new Dictionary<string, object>
                    {
                        { "Result", $"{rowsAffected} row(s) affected." }
                    };
                    QueryResults.Add(resultRow);
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL query");
                ErrorMessage = $"Error: {ex.Message}";
                return Page();
            }
        }
    }
}
