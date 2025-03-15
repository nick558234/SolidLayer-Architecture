using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Models
{
    public class Restaurant
    {
        public string RestaurantID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;

        // Navigation property
        public User User { get; set; } = new User();

        // Many-to-many relationship with Category
        public ICollection<RestaurantCategory> Categories { get; set; } = new List<RestaurantCategory>();

        // Many-to-many relationship with Dish
        public ICollection<Swipe2TryCore.Models.Dish> Dishes { get; set; } = new List<Swipe2TryCore.Models.Dish>();
    }
}
