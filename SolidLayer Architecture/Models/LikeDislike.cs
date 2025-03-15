using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Models
{
    public class LikeDislike
    {
        public string LikeDislikeID { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;
        public string DishID { get; set; } = string.Empty;
        public bool IsLike { get; set; }

        public User User { get; set; } = new User();
        public Swipe2TryCore.Models.Dish Dish { get; set; } = new Swipe2TryCore.Models.Dish();
    }
}
