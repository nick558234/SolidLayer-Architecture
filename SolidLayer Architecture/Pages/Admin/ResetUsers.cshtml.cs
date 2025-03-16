using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace SolidLayer_Architecture.Pages.Admin
{
    public class ResetUsersModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResetUsersModel> _logger;

        public ResetUsersModel(IConfiguration configuration, ILogger<ResetUsersModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
            StatusMessage = string.Empty;
            ErrorMessage = string.Empty;
        }

        [TempData]
        public string StatusMessage { get; set; }
        
        [TempData]
        public string ErrorMessage { get; set; }
        
        public bool ResetComplete { get; private set; }

        public void OnGet()
        {
        }

        public IActionResult OnPostReset()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    ErrorMessage = "Connection string not found.";
                    return Page();
                }
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    // Reset default users' passwords
                    ResetDefaultUserPasswords(connection);
                }
                
                StatusMessage = "User passwords have been reset successfully.";
                ResetComplete = true;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting user passwords");
                ErrorMessage = $"Error: {ex.Message}";
                return Page();
            }
        }
        
        private void ResetDefaultUserPasswords(SqlConnection connection)
        {
            // Admin: admin123
            string adminPasswordHash = HashPassword("admin123");
            UpdateUserPassword(connection, "admin@swipe2try.com", adminPasswordHash);
            
            // Restaurant Owner: password123
            string restaurantOwnerPasswordHash = HashPassword("password123");
            UpdateUserPassword(connection, "restaurant@example.com", restaurantOwnerPasswordHash);
            
            // Regular User: password123
            string userPasswordHash = HashPassword("password123");
            UpdateUserPassword(connection, "user@example.com", userPasswordHash);
        }
        
        private void UpdateUserPassword(SqlConnection connection, string email, string passwordHash)
        {
            string updateSql = "UPDATE USERS SET Password = @Password WHERE Email = @Email";
            
            using (SqlCommand command = new SqlCommand(updateSql, connection))
            {
                command.Parameters.AddWithValue("@Password", passwordHash);
                command.Parameters.AddWithValue("@Email", email);
                command.ExecuteNonQuery();
            }
            
            _logger.LogInformation("Reset password for user: {Email}", email);
        }
        
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
