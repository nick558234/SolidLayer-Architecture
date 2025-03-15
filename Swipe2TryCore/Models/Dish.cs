namespace Swipe2TryCore.Models
{
    public class Dish
    {
        public string DishID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public string HealthFactor { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
        public ICollection<LikeDislike> LikeDislikes { get; set; } = new List<LikeDislike>();
    }
}
