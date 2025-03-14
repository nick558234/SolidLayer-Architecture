using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.RestaurantOwner
{
    public class EditDishModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILogger<EditDishModel> _logger;

        public EditDishModel(IDishService dishService, ILogger<EditDishModel> logger)
        {
            _dishService = dishService;
            _logger = logger;
        }

        [BindProperty]
        public Dish? Dish { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        // Restaurant ID would normally come from authentication
        private const string RestaurantId = "1";

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Edit dish requested without an ID");
                return RedirectToPage("Dashboard");
            }

            Dish = _dishService.GetDishById(id);
            if (Dish == null)
            {
                _logger.LogWarning("Dish with ID {DishId} not found for editing by restaurant owner", id);
                return Page();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                if (Dish == null)
                {
                    return RedirectToPage("Dashboard");
                }

                // Validate input
                if (string.IsNullOrEmpty(Dish.Name))
                {
                    ModelState.AddModelError("Dish.Name", "Name is required");
                }

                // Remove validation errors for collection properties
                ModelState.Remove("Dish.Categories");
                ModelState.Remove("Dish.Restaurants");
                ModelState.Remove("Dish.LikeDislikes");

                if (!ModelState.IsValid)
                {
                    ErrorMessage = "Please correct the errors below.";
                    return Page();
                }

                // Ensure ID is not too long
                if (Dish.DishID.Length > 10)
                {
                    Dish.DishID = Dish.DishID.Substring(0, 10);
                }

                // Ensure HealthFactor isn't too long and normalize it
                if (!string.IsNullOrEmpty(Dish.HealthFactor))
                {
                    if (Dish.HealthFactor.Length > 20)
                    {
                        Dish.HealthFactor = Dish.HealthFactor.Substring(0, 20);
                    }
                    
                    // Normalize health factor
                    string normalized = Dish.HealthFactor.ToLower().Trim();
                    
                    if (normalized.Contains("very healthy")) Dish.HealthFactor = "Very Healthy";
                    else if (normalized.Contains("healthy")) Dish.HealthFactor = "Healthy";
                    else if (normalized.Contains("moderate")) Dish.HealthFactor = "Moderate";
                    else if (normalized.Contains("less healthy")) Dish.HealthFactor = "Less Healthy";
                    else if (normalized.Contains("unhealthy")) Dish.HealthFactor = "Unhealthy";
                }

                // Get existing dish to preserve collections
                var existingDish = _dishService.GetDishById(Dish.DishID);
                if (existingDish == null)
                {
                    ErrorMessage = "Dish not found in the database.";
                    return Page();
                }

                // Preserve the collections from the existing dish
                Dish.Categories = existingDish.Categories;
                Dish.Restaurants = existingDish.Restaurants;
                Dish.LikeDislikes = existingDish.LikeDislikes;

                _logger.LogInformation("Restaurant owner updating dish: {DishId} - {Name}", Dish.DishID, Dish.Name);
                _dishService.UpdateDish(Dish);

                StatusMessage = "Dish updated successfully!";
                return RedirectToPage("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish by restaurant owner: {Message}", ex.Message);
                ErrorMessage = $"Error updating dish: {ex.Message}";
                return Page();
            }
        }
    }
}
