using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.RestaurantOwner
{
    public class DishDetailsModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILikeDislikeService _likeDislikeService;
        private readonly ILogger<DishDetailsModel> _logger;

        public DishDetailsModel(
            IDishService dishService,
            ILikeDislikeService likeDislikeService,
            ILogger<DishDetailsModel> logger)
        {
            _dishService = dishService;
            _likeDislikeService = likeDislikeService;
            _logger = logger;
        }

        public Dish? Dish { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int EngagementRate { get; set; }
        public int LikePercentage { get; set; } = 50;
        public int DislikePercentage { get; set; } = 50;
        
        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Dish details requested without an ID in restaurant owner area");
                return RedirectToPage("Dashboard");
            }

            Dish = _dishService.GetDishById(id);
            if (Dish == null)
            {
                _logger.LogWarning("Dish with ID {DishId} not found in restaurant owner details view", id);
                return Page();
            }

            // Get likes and dislikes counts
            LikesCount = _likeDislikeService.GetLikeCount(id);
            DislikesCount = _likeDislikeService.GetDislikeCount(id);

            // Calculate engagement rate
            int totalInteractions = LikesCount + DislikesCount;
            EngagementRate = totalInteractions > 0 ? (int)(totalInteractions * 100.0 / Math.Max(totalInteractions, 1)) : 0;

            // Calculate like/dislike percentages
            if (totalInteractions > 0)
            {
                LikePercentage = (int)(LikesCount * 100.0 / totalInteractions);
                DislikePercentage = 100 - LikePercentage;
            }

            return Page();
        }
    }
}
