using Microsoft.Data.SqlClient;
using SolidLayer_Architecture.Models;
using System.Security.Cryptography;
using System.Text;

namespace SolidLayer_Architecture.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string? _connectionString;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;

            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("Connection string is null or empty");
                throw new InvalidOperationException("Connection string cannot be null or empty");
            }

            // Ensure the USERS and ROLES tables exist
            EnsureTablesExist();
        }

        private void EnsureTablesExist()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    // Check if ROLES table exists first
                    bool rolesTableExists = TableExists(connection, "ROLES");
                    if (!rolesTableExists)
                    {
                        // Create the ROLES table
                        _logger.LogInformation("Creating ROLES table");
                        string createRolesTableSql = @"
                            CREATE TABLE ROLES (
                                RoleID NVARCHAR(10) PRIMARY KEY,
                                RoleName NVARCHAR(50) NOT NULL
                            )";
                            
                        using (SqlCommand command = new SqlCommand(createRolesTableSql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        
                        // Insert default roles
                        InsertDefaultRoles(connection);
                    }
                    else
                    {
                        // Verify that the default roles exist
                        EnsureDefaultRolesExist(connection);
                    }
                    
                    // Now check if USERS table exists
                    bool usersTableExists = TableExists(connection, "USERS");
                    if (!usersTableExists)
                    {
                        // Create the USERS table with a foreign key to ROLES
                        _logger.LogInformation("Creating USERS table");
                        string createUsersTableSql = @"
                            CREATE TABLE USERS (
                                UserID NVARCHAR(10) PRIMARY KEY,
                                Name NVARCHAR(100) NOT NULL,
                                Email NVARCHAR(100) NOT NULL UNIQUE,
                                Password NVARCHAR(100) NOT NULL,
                                RoleID NVARCHAR(10) NOT NULL,
                                CreatedAt DATETIME DEFAULT GETDATE(),
                                FOREIGN KEY (RoleID) REFERENCES ROLES(RoleID)
                            )";
                            
                        using (SqlCommand command = new SqlCommand(createUsersTableSql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        
                        // Insert default users
                        InsertDefaultAdmin(connection);
                        InsertDefaultRestaurantOwner(connection);
                        InsertDefaultUser(connection);
                    }
                    else 
                    {
                        // Ensure default test users exist
                        EnsureDefaultUsersExist(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring auth tables exist");
            }
        }

        private void EnsureDefaultUsersExist(SqlConnection connection)
        {
            try
            {
                // Check for admin user
                bool adminExists = CheckUserExists(connection, "admin@swipentry.com");
                if (!adminExists)
                {
                    InsertDefaultAdmin(connection);
                }
                    
                // Check for restaurant owner
                bool restaurantOwnerExists = CheckUserExists(connection, "restaurant@example.com");
                if (!restaurantOwnerExists)
                {
                    InsertDefaultRestaurantOwner(connection);
                }
                    
                // Check for regular user
                bool regularUserExists = CheckUserExists(connection, "user@example.com");
                if (!regularUserExists)
                {
                    InsertDefaultUser(connection);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring default users exist");
            }
        }
        
        private bool CheckUserExists(SqlConnection connection, string email)
        {
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM USERS WHERE Email = @Email", connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private void InsertDefaultRestaurantOwner(SqlConnection connection)
        {
            try
            {
                // Create restaurant owner with password "password123"
                string hashedPassword = HashPassword("password123");
                    
                string insertSql = @"
                    INSERT INTO USERS (UserID, Name, Email, Password, RoleID) 
                    VALUES ('u2', 'Restaurant Owner', 'restaurant@example.com', @Password, 'r2')";
                    
                using (SqlCommand command = new SqlCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.ExecuteNonQuery();
                }
                    
                _logger.LogInformation("Default restaurant owner user created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting default restaurant owner");
            }
        }

        private void InsertDefaultUser(SqlConnection connection)
        {
            try
            {
                // Create regular user with password "password123"
                string hashedPassword = HashPassword("password123");
                    
                string insertSql = @"
                    INSERT INTO USERS (UserID, Name, Email, Password, RoleID) 
                    VALUES ('u3', 'Regular User', 'user@example.com', @Password, 'r3')";
                    
                using (SqlCommand command = new SqlCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.ExecuteNonQuery();
                }
                    
                _logger.LogInformation("Default regular user created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting default user");
            }
        }
        
        private void EnsureDefaultRolesExist(SqlConnection connection)
        {
            try
            {
                // Check if the default roles exist
                string checkRolesSql = "SELECT COUNT(*) FROM ROLES WHERE RoleID IN ('r1', 'r2', 'r3')";
                int rolesCount = 0;
                
                using (SqlCommand command = new SqlCommand(checkRolesSql, connection))
                {
                    rolesCount = (int)command.ExecuteScalar();
                }
                
                // If we don't have all 3 default roles, insert the missing ones
                if (rolesCount < 3)
                {
                    _logger.LogInformation("Adding missing default roles");
                    
                    // Check for Admin role
                    CheckAndInsertRole(connection, "r1", "Admin");
                    
                    // Check for RestaurantOwner role
                    CheckAndInsertRole(connection, "r2", "RestaurantOwner");
                    
                    // Check for User role
                    CheckAndInsertRole(connection, "r3", "User");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring default roles exist");
            }
        }
        
        private void CheckAndInsertRole(SqlConnection connection, string roleId, string roleName)
        {
            try
            {
                string checkRoleSql = "SELECT COUNT(*) FROM ROLES WHERE RoleID = @RoleID";
                int roleExists = 0;
                
                using (SqlCommand command = new SqlCommand(checkRoleSql, connection))
                {
                    command.Parameters.AddWithValue("@RoleID", roleId);
                    roleExists = (int)command.ExecuteScalar();
                }
                
                if (roleExists == 0)
                {
                    string insertRoleSql = "INSERT INTO ROLES (RoleID, RoleName) VALUES (@RoleID, @RoleName)";
                    using (SqlCommand command = new SqlCommand(insertRoleSql, connection))
                    {
                        command.Parameters.AddWithValue("@RoleID", roleId);
                        command.Parameters.AddWithValue("@RoleName", roleName);
                        command.ExecuteNonQuery();
                        _logger.LogInformation("Inserted missing {roleName} role", roleName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking/inserting role {roleId}", roleId);
            }
        }
        
        private void InsertDefaultRoles(SqlConnection connection)
        {
            try
            {
                // Insert default roles
                string insertRolesSql = @"
                    INSERT INTO ROLES (RoleID, RoleName) VALUES 
                    ('r1', 'Admin'), 
                    ('r2', 'RestaurantOwner'), 
                    ('r3', 'User')";
                    
                using (SqlCommand command = new SqlCommand(insertRolesSql, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                _logger.LogInformation("Default roles created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting default roles");
            }
        }
        
        private void InsertDefaultAdmin(SqlConnection connection)
        {
            try
            {
                // Create default admin with password "admin123"
                string hashedPassword = HashPassword("admin123");
                
                string insertAdminSql = @"
                    INSERT INTO USERS (UserID, Name, Email, Password, RoleID) 
                    VALUES ('u1', 'Administrator', 'admin@swipentry.com', @Password, 'r1')";
                    
                using (SqlCommand command = new SqlCommand(insertAdminSql, connection))
                {
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.ExecuteNonQuery();
                }
                
                _logger.LogInformation("Default admin user created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting default admin");
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

        public User? GetUserByEmail(string email)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT u.*, r.RoleName 
                        FROM USERS u
                        JOIN ROLES r ON u.RoleID = r.RoleID
                        WHERE u.Email = @Email";
                        
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapUserFromReader(reader);
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return null;
            }
        }

        public User? GetUserById(string id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT u.*, r.RoleName 
                        FROM USERS u
                        JOIN ROLES r ON u.RoleID = r.RoleID
                        WHERE u.UserID = @UserID";
                        
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", id);
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapUserFromReader(reader);
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {ID}", id);
                return null;
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT u.*, r.RoleName 
                        FROM USERS u
                        JOIN ROLES r ON u.RoleID = r.RoleID";
                        
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(MapUserFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
            }
            
            return users;
        }

        public void CreateUser(User user)
        {
            try
            {
                // Generate a new user ID if not provided
                if (string.IsNullOrEmpty(user.UserID))
                {
                    user.UserID = $"u{DateTime.Now.Ticks % 900000 + 100000}";
                }
                
                // Validate role exists before inserting
                if (!RoleExists(user.RoleID))
                {
                    _logger.LogWarning("Attempted to create user with non-existent role: {roleId}. Using default User role.", user.RoleID);
                    user.RoleID = "r3"; // Default to User role
                }
                
                // Hash the password
                string hashedPassword = HashPassword(user.Password);
                
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string insertSql = @"
                        INSERT INTO USERS (UserID, Name, Email, Password, RoleID)
                        VALUES (@UserID, @Name, @Email, @Password, @RoleID)";
                        
                    using (SqlCommand command = new SqlCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", user.UserID);
                        command.Parameters.AddWithValue("@Name", user.Name);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@Password", hashedPassword);
                        command.Parameters.AddWithValue("@RoleID", user.RoleID);
                        
                        command.ExecuteNonQuery();
                    }
                }
                
                _logger.LogInformation("User created: {UserID}, {Email}", user.UserID, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}", user.Email);
                throw;
            }
        }

        public void UpdateUser(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string updateSql = @"
                        UPDATE USERS
                        SET Name = @Name,
                            Email = @Email,
                            RoleID = @RoleID
                        WHERE UserID = @UserID";
                    
                    // If password is provided, update it too
                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        string hashedPassword = HashPassword(user.Password);
                        updateSql = @"
                            UPDATE USERS
                            SET Name = @Name,
                                Email = @Email,
                                Password = @Password,
                                RoleID = @RoleID
                            WHERE UserID = @UserID";
                        
                        using (SqlCommand command = new SqlCommand(updateSql, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", user.UserID);
                            command.Parameters.AddWithValue("@Name", user.Name);
                            command.Parameters.AddWithValue("@Email", user.Email);
                            command.Parameters.AddWithValue("@Password", hashedPassword);
                            command.Parameters.AddWithValue("@RoleID", user.RoleID);
                            
                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (SqlCommand command = new SqlCommand(updateSql, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", user.UserID);
                            command.Parameters.AddWithValue("@Name", user.Name);
                            command.Parameters.AddWithValue("@Email", user.Email);
                            command.Parameters.AddWithValue("@RoleID", user.RoleID);
                            
                            command.ExecuteNonQuery();
                        }
                    }
                }
                
                _logger.LogInformation("User updated: {UserID}, {Email}", user.UserID, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserID}", user.UserID);
                throw;
            }
        }

        public bool IsEmailTaken(string email)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = "SELECT COUNT(*) FROM USERS WHERE Email = @Email";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email is taken: {Email}", email);
                return false;
            }
        }

        public User? ValidateUser(string email, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT u.*, r.RoleName 
                        FROM USERS u
                        JOIN ROLES r ON u.RoleID = r.RoleID
                        WHERE u.Email = @Email";
                        
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPassword = reader["Password"].ToString() ?? string.Empty;
                                
                                // Verify the password
                                if (VerifyPassword(password, storedPassword))
                                {
                                    return MapUserFromReader(reader);
                                }
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user: {Email}", email);
                return null;
            }
        }

        public IEnumerable<Role> GetAllRoles()
        {
            List<Role> roles = new List<Role>();
            
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = "SELECT RoleID, RoleName FROM ROLES";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Role role = new Role
                                {
                                    RoleID = reader["RoleID"].ToString() ?? string.Empty,
                                    RoleName = reader["RoleName"].ToString() ?? string.Empty
                                };
                                
                                roles.Add(role);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles");
            }
            
            return roles;
        }

        public Role? GetRoleById(string id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = "SELECT RoleID, RoleName FROM ROLES WHERE RoleID = @RoleID";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoleID", id);
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Role
                                {
                                    RoleID = reader["RoleID"].ToString() ?? string.Empty,
                                    RoleName = reader["RoleName"].ToString() ?? string.Empty
                                };
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role by ID: {ID}", id);
                return null;
            }
        }

        private User MapUserFromReader(SqlDataReader reader)
        {
            return new User
            {
                UserID = reader["UserID"].ToString() ?? string.Empty,
                Name = reader["Name"].ToString() ?? string.Empty,
                Email = reader["Email"].ToString() ?? string.Empty,
                Password = reader["Password"].ToString() ?? string.Empty, // This is the hashed password
                RoleID = reader["RoleID"].ToString() ?? string.Empty,
                Role = new Role
                {
                    RoleID = reader["RoleID"].ToString() ?? string.Empty,
                    RoleName = reader["RoleName"].ToString() ?? string.Empty
                }
            };
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
        
        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            
            // Create a StringComparer for a case-insensitive comparison
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            
            return comparer.Compare(hashOfInput, hashedPassword) == 0;
        }
        
        private bool RoleExists(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return false;
            }
            
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = "SELECT COUNT(*) FROM ROLES WHERE RoleID = @RoleID";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoleID", roleId);
                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if role exists: {RoleID}", roleId);
                return false;
            }
        }
    }
}
