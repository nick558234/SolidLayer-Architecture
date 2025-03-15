using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Models
{
    public class DishCategory
    {
        public string DishID { get; set; } = string.Empty;
        public string CategoryID { get; set; } = string.Empty;

        // Navigation properties
        public Swipe2TryCore.Models.Dish Dish { get; set; } = new Swipe2TryCore.Models.Dish();
        public Category Category { get; set; } = new Category();
    }
}
