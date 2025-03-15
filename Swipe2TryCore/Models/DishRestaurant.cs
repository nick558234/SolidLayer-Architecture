using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Swipe2TryCore.Models
{
    public class DishRestaurant
    {
        public string? DishID { get; set; } = string.Empty;
        public Dish Dish { get; set; } = new Dish();
        public string? RestaurantID { get; set; } = string.Empty;
        public Restaurant Restaurant { get; set; } = new Restaurant();
        public string Photo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
