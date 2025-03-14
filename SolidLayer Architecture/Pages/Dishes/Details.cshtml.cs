using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.Dishes
{
    public class DetailsModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILikeDislikeService _likeDislikeService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IDishService dishService, 
            ILikeDislikeService likeDislikeService,
            ILogger<DetailsModel> logger)
        {
            _dishService = dishService;
            _likeDislikeService = likeDislikeService;
            _logger = logger;
        }

        public Dish? Dish { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        
        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Dish details requested without an ID");
                return RedirectToPage("/Index");
            }

            Dish = _dishService.GetDishById(id);
            if (Dish == null)
            {
                _logger.LogWarning("Dish with ID {DishId} not found", id);
                return Page();
            }

            // Get likes and dislikes counts
            LikesCount = _likeDislikeService.GetLikeCount(id);
            DislikesCount = _likeDislikeService.GetDislikeCount(id);

            return Page();
        }
    }
}
