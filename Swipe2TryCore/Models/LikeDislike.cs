namespace Swipe2TryCore.Models
{
    public class LikeDislike
    {
        public string LikeDislikeID { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;
        public string DishID { get; set; } = string.Empty;
        public bool IsLike { get; set; }

        // Navigation properties
        public User User { get; set; } = new User();
        public Dish Dish { get; set; } = new Dish();
    }
}
