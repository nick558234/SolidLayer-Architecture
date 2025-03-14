namespace SolidLayer_Architecture.Models
{
    public class User
    {
        public string UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RoleID { get; set; }

        // Navigation property
        public Role Role { get; set; }
    }

}
