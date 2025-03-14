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

        public IActionResult OnGet()
        {
            // Generate a clean ID that won't cause problems
            string newId = GenerateCleanId();
            _logger.LogInformation("Generated new dish ID for regular create: {DishId}", newId);
            
            // Ensure collections are initialized
            Dish = new Dish
            {
                DishID = newId,
                Categories = new List<Category>(),
                Restaurants = new List<Restaurant>(),
                LikeDislikes = new List<LikeDislike>()
            };
            
            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                // Add minimal validation
                if (string.IsNullOrEmpty(Dish.Name))
                {
                    ModelState.AddModelError("Dish.Name", "Name is required");
                }
                
                // Remove validation errors for collection properties that will be initialized
                ModelState.Remove("Dish.Categories");
                ModelState.Remove("Dish.Restaurants");
                ModelState.Remove("Dish.LikeDislikes");

                if (!ModelState.IsValid)
                {
                    ErrorMessage = "Please fix the validation errors.";
                    return Page();
                }

                // Generate a clean ID to avoid issues
                if (string.IsNullOrEmpty(Dish.DishID) || Dish.DishID.Contains("-"))
                {
                    Dish.DishID = GenerateCleanId();
                }
                else if (Dish.DishID.Length > 10)
                {
                    // Keep only the first 10 chars if too long
                    Dish.DishID = Dish.DishID.Substring(0, 10);
                }

                // Ensure HealthFactor isn't too long (max 20 chars) and normalize it
                if (!string.IsNullOrEmpty(Dish.HealthFactor))
                {
                    if (Dish.HealthFactor.Length > 20)
                    {
                        Dish.HealthFactor = Dish.HealthFactor.Substring(0, 20);
                    }
                    
                    // Normalize health factor values
                    Dish.HealthFactor = NormalizeHealthFactor(Dish.HealthFactor);
                }

                // Initialize all collections to avoid null reference exceptions
                Dish.Categories ??= new List<Category>();
                Dish.Restaurants ??= new List<Restaurant>();
                Dish.LikeDislikes ??= new List<LikeDislike>();

                _logger.LogInformation("Attempting to create dish: {DishName} with ID: {DishId}", Dish.Name, Dish.DishID);

                _dishService.AddDish(Dish);
                
                StatusMessage = "Dish created successfully!";
                _logger.LogInformation("Dish created successfully: {DishId} - {DishName}", Dish.DishID, Dish.Name);
                
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dish: {Message}", ex.Message);
                ErrorMessage = $"Error creating dish: {ex.Message}";
                ModelState.AddModelError(string.Empty, $"Error creating dish: {ex.Message}");
                return Page();
            }
        }
        
        // Generate a clean ID without hyphens
        private string GenerateCleanId()
        {
            // Use a format like d100001, d100002, etc.
            return $"d{DateTime.Now.Ticks % 900000 + 100000}";
        }
        
        // Normalize health factor to standard values
        private string NormalizeHealthFactor(string healthFactor)
        {
            string normalized = healthFactor.ToLower().Trim();
            
            if (normalized.Contains("very healthy")) return "Very Healthy";
            if (normalized.Contains("healthy")) return "Healthy";
            if (normalized.Contains("moderate")) return "Moderate";
            if (normalized.Contains("less healthy")) return "Less Healthy";
            if (normalized.Contains("unhealthy")) return "Unhealthy";
            
            return healthFactor; // Keep as-is if not matching any standard value
        }
    }
}
