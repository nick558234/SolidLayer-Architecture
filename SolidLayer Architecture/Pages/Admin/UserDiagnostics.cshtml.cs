using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SolidLayer_Architecture.Pages.Admin
{
    // Allow anonymous access to this page
    [AllowAnonymous]
    public class UserDiagnosticsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserDiagnosticsModel> _logger;

        public UserDiagnosticsModel(IConfiguration configuration, ILogger<UserDiagnosticsModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public List<Dictionary<string, object>> Users { get; set; } = new List<Dictionary<string, object>>();
        public List<Dictionary<string, object>> Roles { get; set; } = new List<Dictionary<string, object>>();
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
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

                    // Get users
                    string userQuery = @"
                        SELECT u.UserID, u.Name, u.Email, r.RoleName 
                        FROM USERS u
                        JOIN ROLES r ON u.RoleID = r.RoleID";

                    using (SqlCommand command = new SqlCommand(userQuery, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new Dictionary<string, object>
                            {
                                { "UserID", reader["UserID"] },
                                { "Name", reader["Name"] },
                                { "Email", reader["Email"] },
                                { "Role", reader["RoleName"] }
                            };
                            Users.Add(user);
                        }
                    }

                    // Get roles
                    string roleQuery = "SELECT RoleID, RoleName FROM ROLES";
                    using (SqlCommand command = new SqlCommand(roleQuery, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var role = new Dictionary<string, object>
                            {
                                { "RoleID", reader["RoleID"] },
                                { "RoleName", reader["RoleName"] }
                            };
                            Roles.Add(role);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in user diagnostics");
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        public IActionResult OnPostForceSetupUsers()
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

                    // Ensure roles exist
                    EnsureRolesExist(connection);
                    
                    // Create admin user
                    string adminPassword = HashPassword("admin123");
                    UpsertUser(connection, "u1", "Administrator", "admin@swipe2try.com", adminPassword, "r1");
                    
                    // Create restaurant owner
                    string restaurantOwnerPassword = HashPassword("password123");
                    UpsertUser(connection, "u2", "Restaurant Owner", "restaurant@example.com", restaurantOwnerPassword, "r2");
                    
                    // Create regular user
                    string userPassword = HashPassword("password123");
                    UpsertUser(connection, "u3", "Regular User", "user@example.com", userPassword, "r3");
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in force setup users");
                ErrorMessage = $"Error: {ex.Message}";
                return Page();
            }
        }

        private void EnsureRolesExist(SqlConnection connection)
        {
            string[] roleIds = { "r1", "r2", "r3" };
            string[] roleNames = { "Admin", "RestaurantOwner", "User" };

            for (int i = 0; i < roleIds.Length; i++)
            {
                string checkRoleSql = "SELECT COUNT(*) FROM ROLES WHERE RoleID = @RoleID";
                int roleExists = 0;
                
                using (SqlCommand command = new SqlCommand(checkRoleSql, connection))
                {
                    command.Parameters.AddWithValue("@RoleID", roleIds[i]);
                    roleExists = (int)command.ExecuteScalar();
                }
                
                if (roleExists == 0)
                {
                    string insertRoleSql = "INSERT INTO ROLES (RoleID, RoleName) VALUES (@RoleID, @RoleName)";
                    using (SqlCommand command = new SqlCommand(insertRoleSql, connection))
                    {
                        command.Parameters.AddWithValue("@RoleID", roleIds[i]);
                        command.Parameters.AddWithValue("@RoleName", roleNames[i]);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void UpsertUser(SqlConnection connection, string userId, string name, string email, string passwordHash, string roleId)
        {
            try
            {
                // First check if the email already exists
                string checkEmailSql = "SELECT UserID FROM USERS WHERE Email = @Email";
                string? existingUserId = null;
                
                using (SqlCommand command = new SqlCommand(checkEmailSql, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    var result = command.ExecuteScalar();
                    existingUserId = result?.ToString();
                }
                
                // If email exists, update it regardless of the UserID
                if (!string.IsNullOrEmpty(existingUserId))
                {
                    string updateUserSql = @"
                        UPDATE USERS
                        SET Name = @Name, Password = @Password, RoleID = @RoleID
                        WHERE Email = @Email";
                    
                    using (SqlCommand command = new SqlCommand(updateUserSql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", passwordHash);
                        command.Parameters.AddWithValue("@RoleID", roleId);
                        command.ExecuteNonQuery();
                    }
                    _logger.LogInformation("Updated user {Email} with ID {UserId}", email, existingUserId);
                }
                else
                {
                    // Check if the specified UserID is already taken
                    string checkUserIdSql = "SELECT COUNT(*) FROM USERS WHERE UserID = @UserID";
                    bool userIdTaken = false;
                    
                    using (SqlCommand command = new SqlCommand(checkUserIdSql, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        int count = (int)command.ExecuteScalar();
                        userIdTaken = (count > 0);
                    }
                    
                    if (userIdTaken)
                    {
                        // Generate a new unique UserID
                        string newUserId = GenerateUniqueUserId(connection);
                        
                        // Insert with the new ID
                        string insertUserSql = @"
                            INSERT INTO USERS (UserID, Name, Email, Password, RoleID)
                            VALUES (@UserID, @Name, @Email, @Password, @RoleID)";
                        
                        using (SqlCommand command = new SqlCommand(insertUserSql, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", newUserId);
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Email", email);
                            command.Parameters.AddWithValue("@Password", passwordHash);
                            command.Parameters.AddWithValue("@RoleID", roleId);
                            command.ExecuteNonQuery();
                        }
                        
                        _logger.LogInformation("Created user {Email} with new ID {UserId} (original ID {OrigUserId} was taken)", 
                            email, newUserId, userId);
                    }
                    else
                    {
                        // UserID is not taken, create new user with specified ID
                        string insertUserSql = @"
                            INSERT INTO USERS (UserID, Name, Email, Password, RoleID)
                            VALUES (@UserID, @Name, @Email, @Password, @RoleID)";
                        
                        using (SqlCommand command = new SqlCommand(insertUserSql, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", userId);
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Email", email);
                            command.Parameters.AddWithValue("@Password", passwordHash);
                            command.Parameters.AddWithValue("@RoleID", roleId);
                            command.ExecuteNonQuery();
                        }
                        
                        _logger.LogInformation("Created new user {Email} with ID {UserId}", email, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting user {Email}", email);
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
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
