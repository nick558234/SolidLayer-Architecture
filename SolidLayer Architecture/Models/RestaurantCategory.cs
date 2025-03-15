namespace SolidLayer_Architecture.Models
{
    public class RestaurantCategory
    {
        public string RestaurantID { get; set; } = string.Empty;
        public string CategoryID { get; set; } = string.Empty;

        // Navigation properties
        public Restaurant Restaurant { get; set; } = new Restaurant();
        public Category Category { get; set; } = new Category();
    }
}
