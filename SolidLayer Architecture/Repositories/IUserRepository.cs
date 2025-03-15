namespace SolidLayer_Architecture.Repositories
{
    public interface IUserRepository
    {
        Swipe2TryCore.Models.User? GetUserByEmail(string email);
        Swipe2TryCore.Models.User? GetUserById(string id);
        IEnumerable<Swipe2TryCore.Models.User> GetAllUsers();
        void CreateUser(Swipe2TryCore.Models.User user);
        void UpdateUser(Swipe2TryCore.Models.User user);
        bool IsEmailTaken(string email);
        Swipe2TryCore.Models.User? ValidateUser(string email, string password);
        IEnumerable<Swipe2TryCore.Models.Role> GetAllRoles();
        Swipe2TryCore.Models.Role? GetRoleById(string id);
    }
}
