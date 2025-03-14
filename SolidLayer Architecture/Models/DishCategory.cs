namespace SolidLayer_Architecture.Models
{
    public class DishCategory
    {
        public string DishID { get; set; }
        public string CategoryID { get; set; }

        // Navigation properties
        public Dish Dish { get; set; }
        public Category Category { get; set; }
    }

}
