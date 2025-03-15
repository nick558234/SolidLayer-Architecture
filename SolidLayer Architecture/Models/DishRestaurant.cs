using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Models
{
    public class DishRestaurant
    {
        public string DishID { get; set; } = string.Empty;
        public string RestaurantID { get; set; } = string.Empty;

        // Navigation properties
        public Swipe2TryCore.Models.Dish Dish { get; set; } = new Swipe2TryCore.Models.Dish();
        public Restaurant Restaurant { get; set; } = new Restaurant();
        public string Photo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
