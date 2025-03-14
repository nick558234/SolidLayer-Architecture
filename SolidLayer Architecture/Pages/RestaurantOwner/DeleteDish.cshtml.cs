using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.RestaurantOwner
{
    public class DeleteDishModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILikeDislikeService _likeDislikeService;
        private readonly ILogger<DeleteDishModel> _logger;

        public DeleteDishModel(
            IDishService dishService,
            ILikeDislikeService likeDislikeService,
            ILogger<DeleteDishModel> logger)
        {
            _dishService = dishService;
            _likeDislikeService = likeDislikeService;
            _logger = logger;
        }

        [BindProperty]
        public Dish? Dish { get; set; }

        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Delete dish requested without an ID by restaurant owner");
                return RedirectToPage("Dashboard");
            }

            Dish = _dishService.GetDishById(id);
            if (Dish == null)
            {
                _logger.LogWarning("Dish with ID {DishId} not found for deletion by restaurant owner", id);
                return Page();
            }

            // Get likes and dislikes counts
            LikesCount = _likeDislikeService.GetLikeCount(id);
            DislikesCount = _likeDislikeService.GetDislikeCount(id);

            return Page();
        }

        public IActionResult OnPost()
        {
            if (Dish == null || string.IsNullOrEmpty(Dish.DishID))
            {
                return RedirectToPage("Dashboard");
            }

            var dishId = Dish.DishID;
            var dishName = _dishService.GetDishById(dishId)?.Name ?? "Unknown";

            try
            {
                _logger.LogInformation("Restaurant owner deleting dish: {DishId} - {Name}", dishId, dishName);
                _dishService.DeleteDish(dishId);
                _logger.LogInformation("Dish deleted successfully by restaurant owner: {DishId} - {Name}", dishId, dishName);

                TempData["StatusMessage"] = $"Dish '{dishName}' was deleted successfully.";
                return RedirectToPage("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dish {DishId} by restaurant owner: {Message}", dishId, ex.Message);
                ErrorMessage = $"Error deleting dish: {ex.Message}";
                
                // Reload dish information
                Dish = _dishService.GetDishById(dishId);
                LikesCount = _likeDislikeService.GetLikeCount(dishId);
                DislikesCount = _likeDislikeService.GetDislikeCount(dishId);
                
                return Page();
            }
        }
    }
}
