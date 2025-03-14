namespace SolidLayer_Architecture.Models
{
    public class LikeDislike
    {
        public string LikeDislikeID { get; set; }
        public string UserID { get; set; }
        public string DishID { get; set; }
        public bool IsLike { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Dish Dish { get; set; }
    }

}
