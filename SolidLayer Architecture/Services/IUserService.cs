using SolidLayer_Architecture.Models;

namespace SolidLayer_Architecture.Services
{
    public interface IUserService
    {
        User? GetUserByEmail(string email);
        User? GetUserById(string id);
        IEnumerable<User> GetAllUsers();
        void CreateUser(User user);
        void UpdateUser(User user);
        bool IsEmailTaken(string email);
        IEnumerable<Role> GetAllRoles();
        Role? GetRoleById(string id);
    }
}
