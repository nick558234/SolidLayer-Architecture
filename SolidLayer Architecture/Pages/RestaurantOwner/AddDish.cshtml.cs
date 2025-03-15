using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.RestaurantOwner
{
    public class AddDishModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILogger<AddDishModel> _logger;

        public AddDishModel(IDishService dishService, ILogger<AddDishModel> logger)
        {
            _dishService = dishService;
            _logger = logger;
            StatusMessage = string.Empty;
            ErrorMessage = string.Empty;
        }

        [BindProperty]
        public Dish Dish { get; set; } = new Dish
        {
            Categories = new List<Category>(),
            Restaurants = new List<Restaurant>(),
            LikeDislikes = new List<LikeDislike>()
        };

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // Generate a clean ID for the new dish
            string newId = GenerateCleanId();
            _logger.LogInformation("Generated new dish ID: {dishId}", newId);
            
            // Initialize a fresh dish object with the generated ID
            Dish = new Dish
            {
                DishID = newId,
                HealthFactor = "50", // Default to moderate (numeric value)
                Categories = new List<Category>(),
                Restaurants = new List<Restaurant>(),
                LikeDislikes = new List<LikeDislike>()
            };
        }

        public IActionResult OnPost()
        {
            try
            {
                // Perform basic form validation
                if (!ValidateDish())
                {
                    return Page();
                }

                // Prepare the dish for saving
                PrepareForSaving();

                // Save the dish
                _logger.LogInformation("Adding new dish: {name} with ID: {id} and health factor: {healthFactor}", 
                    Dish.Name, Dish.DishID, Dish.HealthFactor);
                _dishService.AddDish(Dish);

                // Set success message and redirect
                StatusMessage = "Dish added successfully!";
                return RedirectToPage("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding dish: {error}", ex.Message);
                ErrorMessage = $"Error adding dish: {ex.Message}";
                return Page();
            }
        }

        #region Helper Methods

        private bool ValidateDish()
        {
            // Make validation less strict - only check required name
            if (string.IsNullOrEmpty(Dish.Name))
            {
                ModelState.AddModelError("Dish.Name", "Dish name is required");
                ErrorMessage = "Please enter a dish name.";
                return false;
            }

            // Clear model state errors for collections
            ModelState.Remove("Dish.Categories");
            ModelState.Remove("Dish.Restaurants");
            ModelState.Remove("Dish.LikeDislikes");

            return true;
        }

        private void PrepareForSaving()
        {
            // Ensure ID is clean and valid
            if (string.IsNullOrEmpty(Dish.DishID) || Dish.DishID.Contains("-"))
            {
                Dish.DishID = GenerateCleanId();
            }
            else if (Dish.DishID.Length > 10)
            {
                // Keep only the first 10 chars if too long
                Dish.DishID = Dish.DishID.Substring(0, 10);
            }

            // Ensure health factor is a valid numeric value between 0-100
            if (!string.IsNullOrEmpty(Dish.HealthFactor))
            {
                if (int.TryParse(Dish.HealthFactor, out int healthValue))
                {
                    // Ensure the value is between 0 and 100
                    healthValue = Math.Max(0, Math.Min(100, healthValue));
                    Dish.HealthFactor = healthValue.ToString();
                }
                else
                {
                    // Default to 50 if not a number
                    Dish.HealthFactor = "50";
                }
            }
            else
            {
                // Default to 50 if empty
                Dish.HealthFactor = "50";
            }

            // Initialize collections to avoid null references
            Dish.Categories ??= new List<Category>();
            Dish.Restaurants ??= new List<Restaurant>();
            Dish.LikeDislikes ??= new List<LikeDislike>();
        }

        private string GenerateCleanId()
        {
            // Format: "d" + 6 digits (timestamp-based to avoid collisions)
            return $"d{DateTime.Now.Ticks % 900000 + 100000}";
        }

        #endregion
    }
}
