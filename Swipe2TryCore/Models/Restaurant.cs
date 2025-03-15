namespace Swipe2TryCore.Models
{
    public class Restaurant
    {
        public string RestaurantID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;

        // Navigation property
        public User User { get; set; } = new User();

        // Many-to-many relationship with categories
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        // Many-to-many relationship with dishes
        public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    }
}
