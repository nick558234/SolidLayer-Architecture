namespace SolidLayer_Architecture.Models
{
    public class RestaurantCategory
    {
        public string RestaurantID { get; set; }
        public string CategoryID { get; set; }

        // Navigation properties
        public Restaurant Restaurant { get; set; }
        public Category Category { get; set; }
    }

}
