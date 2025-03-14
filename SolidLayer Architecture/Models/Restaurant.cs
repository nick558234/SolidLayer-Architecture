namespace SolidLayer_Architecture.Models
{
    public class Restaurant
    {
        public string RestaurantID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string UserID { get; set; }

        // Navigation property
        public User User { get; set; }

        // Many-to-many relationship with categories
        public ICollection<Category> Categories { get; set; }

        // Many-to-many relationship with dishes
        public ICollection<Dish> Dishes { get; set; }
    }

}
