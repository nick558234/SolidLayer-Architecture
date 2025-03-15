using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.Dishes
{
    public class CreateModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IDishService dishService, ILogger<CreateModel> logger)
        {
            _dishService = dishService;
            _logger = logger;
        }

        [BindProperty]
        public Dish Dish { get; set; } = new Dish();

        public IActionResult OnGet()
        {
            Dish = new Dish
            {
                HealthFactor = "50", // Default to moderate (numeric value)
                Categories = new List<Category>(),
                Restaurants = new List<Restaurant>(),
                LikeDislikes = new List<LikeDislike>()
            };
            
            return Page();
        }

        public IActionResult OnPost()
        {
            // Make validation less strict - only check required name
            if (string.IsNullOrEmpty(Dish.Name))
            {
                ModelState.AddModelError("Dish.Name", "Dish name is required");
                return Page();
            }
            
            // Clear validation errors for navigation properties
            ModelState.Remove("Dish.Categories");
            ModelState.Remove("Dish.Restaurants");
            ModelState.Remove("Dish.LikeDislikes");

            // Prepare the dish for saving
            PrepareForSaving();
            
            // Log the health factor before saving
            _logger.LogInformation("Creating dish with health factor: {healthFactor}", Dish.HealthFactor);
            
            _dishService.AddDish(Dish);

            return RedirectToPage("./Index");
        }
        
        private void PrepareForSaving()
        {
            // Ensure ID is clean and valid
            if (string.IsNullOrEmpty(Dish.DishID) || Dish.DishID.Contains("-"))
            {
                Dish.DishID = $"d{DateTime.Now.Ticks % 900000 + 100000}";
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
    }
}
