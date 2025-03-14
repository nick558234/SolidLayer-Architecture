namespace SolidLayer_Architecture.Models
{
    public class Dish
    {
        public string DishID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }
        public string HealthFactor { get; set; }

        // Many-to-many relationship with categories
        public ICollection<Category> Categories { get; set; }

        // Many-to-many relationship with restaurants
        public ICollection<Restaurant> Restaurants { get; set; }

        // Likes and dislikes
        public ICollection<LikeDislike> LikeDislikes { get; set; }
    }

}
