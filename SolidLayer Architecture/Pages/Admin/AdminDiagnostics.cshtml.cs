using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace SolidLayer_Architecture.Pages.Admin
{
    // Allow anonymous access to this page
    [AllowAnonymous]
    public class AdminDiagnosticsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminDiagnosticsModel> _logger;

        public AdminDiagnosticsModel(IConfiguration configuration, ILogger<AdminDiagnosticsModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Dictionary<string, string> AdminUser { get; set; } = new Dictionary<string, string>();
        public bool AdminExists { get; set; } = false;
        public string AdminPasswordDebug { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            CheckAdminUser();
        }

        private void CheckAdminUser()
        {
            try
            {
                string? connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    ErrorMessage = "Connection string not found.";
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for admin user in database
                    string query = @"
                        SELECT u.UserID, u.Name, u.Email, u.Password, u.RoleID, r.RoleName 
                        FROM USERS u
                        JOIN ROLES r ON u.RoleID = r.RoleID
                        WHERE u.Email = 'admin@swipe2try.com'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            AdminExists = true;
                            AdminUser["UserID"] = reader["UserID"].ToString() ?? string.Empty;
                            AdminUser["Name"] = reader["Name"].ToString() ?? string.Empty;
                            AdminUser["Email"] = reader["Email"].ToString() ?? string.Empty;
                            AdminUser["Password"] = reader["Password"].ToString() ?? string.Empty;
                            AdminUser["RoleID"] = reader["RoleID"].ToString() ?? string.Empty;
                            AdminUser["RoleName"] = reader["RoleName"].ToString() ?? string.Empty;

                            // Hash actual password for comparison
                            string expectedHash = HashPassword("admin123");
                            AdminPasswordDebug = $"Expected hash: {expectedHash}\nActual hash: {AdminUser["Password"]}";
                        }
                    }

                    // If admin user doesn't exist, check if the roles table exists and has admin role
                    if (!AdminExists)
                    {
                        // Check if roles table exists
                        bool rolesTableExists = TableExists(connection, "ROLES");
                        AdminUser["RolesTableExists"] = rolesTableExists.ToString();

                        // Check if admin role exists
                        if (rolesTableExists)
                        {
                            string roleQuery = "SELECT COUNT(*) FROM ROLES WHERE RoleID = 'r1'";
                            using (SqlCommand command = new SqlCommand(roleQuery, connection))
                            {
                                int count = (int)command.ExecuteScalar();
                                AdminUser["AdminRoleExists"] = count > 0 ? "Yes" : "No";
                            }
                        }

                        // Check if users table exists
                        bool usersTableExists = TableExists(connection, "USERS");
                        AdminUser["UsersTableExists"] = usersTableExists.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in admin diagnostics");
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        public IActionResult OnPostForceCreateAdmin()
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

                    // Create or update admin user with correct password
                    string adminPassword = HashPassword("admin123");
                    CreateOrUpdateAdminUser(connection, adminPassword);
                }

                StatusMessage = "Admin user has been created/updated successfully!";
                CheckAdminUser(); // Refresh admin user data
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin user");
                ErrorMessage = $"Error: {ex.Message}";
                return Page();
            }
        }

        public IActionResult OnPostTestLogin()
        {
            try
            {
                string? connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    ErrorMessage = "Connection string not found.";
                    return Page();
                }

                string password = "admin123";
                string email = "admin@swipe2try.com";
                bool loginSuccessful = false;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT Password FROM USERS WHERE Email = @Email";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPassword = reader["Password"].ToString() ?? string.Empty;
                                string inputPasswordHash = HashPassword(password);
                                
                                // Case-insensitive comparison
                                loginSuccessful = string.Equals(inputPasswordHash, storedPassword, StringComparison.OrdinalIgnoreCase);

                                AdminPasswordDebug = $"Input hash: {inputPasswordHash}\nStored hash: {storedPassword}\nMatch: {loginSuccessful}";
                            }
                            else
                            {
                                AdminPasswordDebug = "User not found in database";
                            }
                        }
                    }
                }

                StatusMessage = loginSuccessful 
                    ? "Login test successful! The admin credentials are working." 
                    : "Login test failed. See debug info for details.";

                CheckAdminUser(); // Refresh admin user data
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing login");
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
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                // Convert byte array to a string
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
