namespace Swipe2TryCore.Models
{
    public class User
    {
        public string UserID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RoleID { get; set; } = string.Empty;

        // Navigation property
        public Role Role { get; set; } = new Role();
    }
}
