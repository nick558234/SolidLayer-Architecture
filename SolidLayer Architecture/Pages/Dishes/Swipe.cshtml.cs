using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.Dishes
{
    /// <summary>
    /// Page model for the dish swiping feature
    /// </summary>
    public class SwipeModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILikeDislikeService _likeDislikeService;
        private readonly ILogger<SwipeModel> _logger;

        public SwipeModel(
            IDishService dishService, 
            ILikeDislikeService likeDislikeService,
            ILogger<SwipeModel> logger)
        {
            _dishService = dishService;
            _likeDislikeService = likeDislikeService;
            _logger = logger;
        }

        public Dish? CurrentDish { get; private set; }
        public bool HasMoreDishes { get; private set; }
        
        // In a real app, this would come from authentication
        private const string UserId = "1";

        /// <summary>
        /// Gets a dish for swiping, considering user's previous swipes
        /// </summary>
        public IActionResult OnGet()
        {
            try
            {
                // Get user's previous preferences
                var previousPreferences = _likeDislikeService.GetUserPreferences(UserId);
                var previousDishIds = previousPreferences.Select(p => p.DishID).ToHashSet();
                
                // Get all available dishes
                var allDishes = _dishService.GetAllDishes().ToList();
                
                // Filter out dishes the user has already swiped
                var unswipedDishes = allDishes
                    .Where(d => !previousDishIds.Contains(d.DishID))
                    .ToList();
                
                if (unswipedDishes.Any())
                {
                    // Select a random dish the user hasn't swiped yet
                    CurrentDish = GetRandomDish(unswipedDishes);
                    HasMoreDishes = true;
                    
                    _logger.LogInformation("Showing dish for swiping: {dishId} - {name}", 
                        CurrentDish.DishID, CurrentDish.Name);
                }
                else if (allDishes.Any())
                {
                    // If the user has swiped all dishes, we can show a random one again
                    CurrentDish = GetRandomDish(allDishes);
                    HasMoreDishes = true;
                    
                    _logger.LogInformation("User has seen all dishes, showing random dish: {dishId}", 
                        CurrentDish.DishID);
                }
                else
                {
                    // No dishes available at all
                    HasMoreDishes = false;
                    _logger.LogWarning("No dishes available for swiping");
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing swipe page");
                HasMoreDishes = false;
                return Page();
            }
        }

        /// <summary>
        /// Gets a random dish from a list of dishes
        /// </summary>
        private Dish GetRandomDish(List<Dish> dishes)
        {
            Random random = new Random();
            int index = random.Next(dishes.Count);
            return dishes[index];
        }
    }
}
