using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.Dishes
{
    public class EditModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IDishService dishService, ILogger<EditModel> logger)
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

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Edit dish requested without an ID");
                return RedirectToPage("/Index");
            }

            Dish = _dishService.GetDishById(id);
            if (Dish == null)
            {
                _logger.LogWarning("Dish with ID {DishId} not found for editing", id);
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
                    return RedirectToPage("/Index");
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

                // Ensure HealthFactor isn't too long
                if (!string.IsNullOrEmpty(Dish.HealthFactor) && Dish.HealthFactor.Length > 20)
                {
                    Dish.HealthFactor = Dish.HealthFactor.Substring(0, 20);
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

                _logger.LogInformation("Updating dish: {DishId} - {Name}", Dish.DishID, Dish.Name);
                _dishService.UpdateDish(Dish);

                StatusMessage = "Dish updated successfully!";
                return RedirectToPage("Details", new { id = Dish.DishID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish: {Message}", ex.Message);
                ErrorMessage = $"Error updating dish: {ex.Message}";
                return Page();
            }
        }
    }
}
