using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.Dishes
{
    public class IndexModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILikeDislikeService _likeDislikeService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IDishService dishService,
            ILikeDislikeService likeDislikeService,
            ILogger<IndexModel> logger)
        {
            _dishService = dishService;
            _likeDislikeService = likeDislikeService;
            _logger = logger;
        }

        public IList<Dish> Dishes { get; private set; } = new List<Dish>();
        public Dictionary<string, int> LikeCounts { get; private set; } = new Dictionary<string, int>();
        public Dictionary<string, int> DislikeCounts { get; private set; } = new Dictionary<string, int>();

        public void OnGet()
        {
            try
            {
                Dishes = _dishService.GetAllDishes().ToList();
                
                // Get likes and dislikes counts for each dish
                foreach (var dish in Dishes)
                {
                    LikeCounts[dish.DishID] = _likeDislikeService.GetLikeCount(dish.DishID);
                    DislikeCounts[dish.DishID] = _likeDislikeService.GetDislikeCount(dish.DishID);
                }
                
                _logger.LogInformation("Retrieved {Count} dishes for management view", Dishes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dishes for management view");
                TempData["ErrorMessage"] = $"Error loading dishes: {ex.Message}";
            }
        }
    }
}
