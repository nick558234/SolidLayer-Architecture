using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace SolidLayer_Architecture.Pages
{
    [AllowAnonymous]
    public class FixAdminModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FixAdminModel> _logger;
        
        public string StatusMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public FixAdminModel(IConfiguration configuration, ILogger<FixAdminModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
            // Nothing needed on GET
        }

        public IActionResult OnPost()
        {
            try
            {
                string? connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    ErrorMessage = "Connection string not found.";
                    return Page();
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Ensure roles table exists
                    EnsureRolesTableExists(connection);

                    // Ensure admin role exists
                    EnsureAdminRoleExists(connection);

                    // Create or update admin user with admin123 password
                    string adminPassword = HashPassword("admin123");
                    CreateOrUpdateAdminUser(connection, adminPassword);
                }

                StatusMessage = "Admin user has been fixed! You can now login with admin@swipe2try.com / admin123";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting admin user");
                ErrorMessage = $"Error: {ex.Message}";
                return Page();
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

        private void EnsureRolesTableExists(SqlConnection connection)
        {
            if (!TableExists(connection, "ROLES"))
            {
                string createRolesSql = @"
                    CREATE TABLE ROLES (
                        RoleID NVARCHAR(10) PRIMARY KEY,
                        RoleName NVARCHAR(50) NOT NULL
                    )";

                using (SqlCommand command = new SqlCommand(createRolesSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void EnsureAdminRoleExists(SqlConnection connection)
        {
            string query = "SELECT COUNT(*) FROM ROLES WHERE RoleID = 'r1'";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                int count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    string insertRoleSql = "INSERT INTO ROLES (RoleID, RoleName) VALUES ('r1', 'Admin')";
                    using (SqlCommand insertCommand = new SqlCommand(insertRoleSql, connection))
                    {
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        private void CreateOrUpdateAdminUser(SqlConnection connection, string passwordHash)
        {
            try
            {
                // First check if any user with UserID = 'u1' exists (regardless of email)
                string checkUserIdSql = "SELECT COUNT(*) FROM USERS WHERE UserID = 'u1'";
                int userIdExists = 0;
                
                using (SqlCommand command = new SqlCommand(checkUserIdSql, connection))
                {
                    userIdExists = (int)command.ExecuteScalar();
                }

                // Then check if admin@swipe2try.com exists but with a different UserID
                string checkEmailSql = "SELECT UserID FROM USERS WHERE Email = 'admin@swipe2try.com'";
                string? existingUserId = null;
                
                using (SqlCommand command = new SqlCommand(checkEmailSql, connection))
                {
                    var result = command.ExecuteScalar();
                    existingUserId = result?.ToString();
                }

                if (userIdExists > 0 && existingUserId != "u1")
                {
                    // UserID 'u1' is taken by another user, we need to update the admin email account instead
                    // First, get a unique UserID that's not already in use
                    string uniqueUserId = GenerateUniqueUserId(connection);
                    
                    // Update the admin user with the new ID or create if it doesn't exist
                    if (!string.IsNullOrEmpty(existingUserId))
                    {
                        // Admin email exists but has a different ID, update it
                        string updateSql = @"
                            UPDATE USERS 
                            SET Name = 'Administrator', Password = @Password, RoleID = 'r1'
                            WHERE Email = 'admin@swipe2try.com'";
                        
                        using (SqlCommand command = new SqlCommand(updateSql, connection))
                        {
                            command.Parameters.AddWithValue("@Password", passwordHash);
                            command.ExecuteNonQuery();
                        }
                        
                        _logger.LogInformation("Updated admin user with existing ID {UserId}", existingUserId);
                    }
                    else
                    {
                        // Admin email doesn't exist, create with a new unique ID
                        string insertSql = @"
                            INSERT INTO USERS (UserID, Name, Email, Password, RoleID)
                            VALUES (@UserID, 'Administrator', 'admin@swipe2try.com', @Password, 'r1')";
                        
                        using (SqlCommand command = new SqlCommand(insertSql, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", uniqueUserId);
                            command.Parameters.AddWithValue("@Password", passwordHash);
                            command.ExecuteNonQuery();
                        }
                        
                        _logger.LogInformation("Created new admin user with ID {UserId}", uniqueUserId);
                    }
                }
                else if (existingUserId == "u1")
                {
                    // The admin user exists with the correct ID, just update the password
                    string updateSql = @"
                        UPDATE USERS 
                        SET Name = 'Administrator', Password = @Password, RoleID = 'r1'
                        WHERE UserID = 'u1'";
                    
                    using (SqlCommand command = new SqlCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@Password", passwordHash);
                        command.ExecuteNonQuery();
                    }
                    
                    _logger.LogInformation("Updated existing admin user with ID u1");
                }
                else
                {
                    // UserID 'u1' is not taken and admin email doesn't exist, create new
                    string insertSql = @"
                        INSERT INTO USERS (UserID, Name, Email, Password, RoleID)
                        VALUES ('u1', 'Administrator', 'admin@swipe2try.com', @Password, 'r1')";
                    
                    using (SqlCommand command = new SqlCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@Password", passwordHash);
                        command.ExecuteNonQuery();
                    }
                    
                    _logger.LogInformation("Created new admin user with ID u1");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating admin user");
                throw;
            }
        }

        private string GenerateUniqueUserId(SqlConnection connection)
        {
            // Generate a unique user ID that doesn't exist in the database
            for (int i = 1; i <= 999; i++)
            {
                string candidateId = $"u{i}";
                string checkSql = "SELECT COUNT(*) FROM USERS WHERE UserID = @UserID";
                
                using (SqlCommand command = new SqlCommand(checkSql, connection))
                {
                    command.Parameters.AddWithValue("@UserID", candidateId);
                    int count = (int)command.ExecuteScalar();
                    
                    if (count == 0)
                    {
                        return candidateId;
                    }
                }
            }
            
            // If all IDs u1-u999 are taken, generate a random one
            return $"u{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
