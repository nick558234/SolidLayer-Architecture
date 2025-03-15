using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.RestaurantOwner
{
    /// <summary>
    /// Page model for showing detailed dish information with analytics
    /// </summary>
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

        // Dish information
        public Dish? Dish { get; set; }
        
        // Analytics data
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int EngagementRate { get; set; }
        public int LikePercentage { get; set; } = 50;
        public int DislikePercentage { get; set; } = 50;
        
        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Dish details requested without an ID");
                return RedirectToPage("Dashboard");
            }

            // Load the dish
            Dish = _dishService.GetDishById(id);
            
            if (Dish == null)
            {
                _logger.LogWarning("Dish with ID {dishId} not found", id);
                return Page();
            }

            // Calculate analytics for the dish
            CalculateAnalytics(id);

            return Page();
        }
        
        /// <summary>
        /// Calculates various analytics metrics for the dish
        /// </summary>
        private void CalculateAnalytics(string dishId)
        {
            // Get basic feedback counts
            LikesCount = _likeDislikeService.GetLikeCount(dishId);
            DislikesCount = _likeDislikeService.GetDislikeCount(dishId);
            
            // Calculate total interactions
            int totalInteractions = LikesCount + DislikesCount;
            
            // Calculate user engagement rate (each user who interacted as percentage of total users)
            // For simplicity, we just use the interaction count now
            EngagementRate = totalInteractions > 0 ? 100 : 0;

            // Calculate like/dislike percentages
            if (totalInteractions > 0)
            {
                LikePercentage = (int)(LikesCount * 100.0 / totalInteractions);
                DislikePercentage = 100 - LikePercentage;
            }
            else
            {
                // Default values when there are no interactions
                LikePercentage = 50;
                DislikePercentage = 50;
            }
        }
    }
}
