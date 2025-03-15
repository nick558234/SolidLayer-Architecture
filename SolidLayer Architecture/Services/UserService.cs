using Swipe2TryCore.Models; // Use Swipe2TryCore models
using SolidLayer_Architecture.Repositories;

namespace SolidLayer_Architecture.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public User? GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("GetUserByEmail called with null or empty email");
                return null;
            }

            try
            {
                return _userRepository.GetUserByEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return null;
            }
        }

        public User? GetUserById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("GetUserById called with null or empty id");
                return null;
            }

            try
            {
                return _userRepository.GetUserById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by id: {Id}", id);
                return null;
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            try
            {
                return _userRepository.GetAllUsers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return Enumerable.Empty<User>();
            }
        }

        public void CreateUser(User user)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogWarning("CreateUser called with null user");
                    throw new ArgumentNullException(nameof(user));
                }

                if (string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning("CreateUser called with null or empty email");
                    throw new ArgumentException("Email is required", nameof(user));
                }

                if (string.IsNullOrEmpty(user.Password))
                {
                    _logger.LogWarning("CreateUser called with null or empty password");
                    throw new ArgumentException("Password is required", nameof(user));
                }

                if (IsEmailTaken(user.Email))
                {
                    _logger.LogWarning("Email already exists: {Email}", user.Email);
                    throw new ArgumentException($"Email '{user.Email}' is already taken", nameof(user));
                }

                // If no role is specified, default to User role
                if (string.IsNullOrEmpty(user.RoleID))
                {
                    user.RoleID = "r3"; // Default role ID for User
                }

                _userRepository.CreateUser(user);
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is ArgumentException))
            {
                _logger.LogError(ex, "Error creating user: {Email}", user?.Email);
                throw;
            }
        }

        public void UpdateUser(User user)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogWarning("UpdateUser called with null user");
                    throw new ArgumentNullException(nameof(user));
                }

                if (string.IsNullOrEmpty(user.UserID))
                {
                    _logger.LogWarning("UpdateUser called with null or empty user ID");
                    throw new ArgumentException("User ID is required", nameof(user));
                }

                if (string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning("UpdateUser called with null or empty email");
                    throw new ArgumentException("Email is required", nameof(user));
                }

                // Check if a different user already has this email
                var existingUser = _userRepository.GetUserByEmail(user.Email);
                if (existingUser != null && existingUser.UserID != user.UserID)
                {
                    _logger.LogWarning("Email already exists for a different user: {Email}", user.Email);
                    throw new ArgumentException($"Email '{user.Email}' is already taken by another user", nameof(user));
                }

                _userRepository.UpdateUser(user);
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is ArgumentException))
            {
                _logger.LogError(ex, "Error updating user: {UserID}", user?.UserID);
                throw;
            }
        }

        public bool IsEmailTaken(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            try
            {
                return _userRepository.IsEmailTaken(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email is taken: {Email}", email);
                return false;
            }
        }

        public IEnumerable<Role> GetAllRoles()
        {
            try
            {
                return _userRepository.GetAllRoles();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles");
                return Enumerable.Empty<Role>();
            }
        }

        public Role? GetRoleById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("GetRoleById called with null or empty id");
                return null;
            }

            try
            {
                return _userRepository.GetRoleById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role by id: {Id}", id);
                return null;
            }
        }
    }
}
