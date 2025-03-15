using SolidLayer_Architecture.Models;

namespace SolidLayer_Architecture.Repositories
{
    public interface IUserRepository
    {
        User? GetUserByEmail(string email);
        User? GetUserById(string id);
        IEnumerable<User> GetAllUsers();
        void CreateUser(User user);
        void UpdateUser(User user);
        bool IsEmailTaken(string email);
        User? ValidateUser(string email, string password);
        IEnumerable<Role> GetAllRoles();
        Role? GetRoleById(string id);
    }
}
