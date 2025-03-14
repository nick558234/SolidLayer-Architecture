using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolidLayer_Architecture.Models
{
    public class DishRestaurant
    {

        public string? DishID { get; set; }
        public Dish Dish { get; set; }
        public string? RestaurantID { get; set; }
        public Restaurant Restaurant { get; set; }
        public string Photo { get; set; }
        public string Description { get; set; }
    }
}
