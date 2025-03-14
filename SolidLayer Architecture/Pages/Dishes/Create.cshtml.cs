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

        // Helper method to ensure ID is within allowed length
        private string EnsureIdLength(string id, int maxLength = 10)
        {
            if (string.IsNullOrEmpty(id))
                return Guid.NewGuid().ToString().Substring(0, maxLength);
                
            return id.Length <= maxLength ? id : id.Substring(0, maxLength);
        }

        public IActionResult OnGet()
        {
            // Ensure collections are initialized
            Dish = new Dish
            {
                // Generate a shorter ID that will fit in the database column
                DishID = EnsureIdLength(Guid.NewGuid().ToString()),
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

                // Ensure DishID is set and not too long
                if (string.IsNullOrEmpty(Dish.DishID))
                {
                    Dish.DishID = EnsureIdLength(Guid.NewGuid().ToString());
                }
                else
                {
                    Dish.DishID = EnsureIdLength(Dish.DishID);
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
    }
}
