using Swipe2TryCore.Models;

namespace Swipe2TryCore.Models
{
    public class DishCategory
    {
        public string DishID { get; set; } = string.Empty;
        public string CategoryID { get; set; } = string.Empty;

        // Navigation properties
        public Dish Dish { get; set; } = new Dish();
        public Category Category { get; set; } = new Category();
    }

}
