using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.RestaurantOwner
{
    public class DishWithStats
    {
        public Dish Dish { get; set; } = null!; // Using null-forgiving operator
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }

    public class DashboardModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILikeDislikeService _likeDislikeService;

        public DashboardModel(IDishService dishService, ILikeDislikeService likeDislikeService)
        {
            _dishService = dishService;
            _likeDislikeService = likeDislikeService;
        }

        public List<DishWithStats> RestaurantDishes { get; set; } = new List<DishWithStats>();
        public int TotalDishes { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public int EngagementRate { get; set; }

        // In a real app, this would come from authentication
        private const string RestaurantOwnerId = "1"; 

        public void OnGet()
        {
            // For now, show all dishes since we don't have restaurant ownership yet
            var dishes = _dishService.GetAllDishes().ToList();
            TotalDishes = dishes.Count;
            
            // Calculate statistics
            TotalLikes = 0;
            TotalDislikes = 0;

            foreach (var dish in dishes)
            {
                var likes = _likeDislikeService.GetLikeCount(dish.DishID);
                var dislikes = _likeDislikeService.GetDislikeCount(dish.DishID);
                
                TotalLikes += likes;
                TotalDislikes += dislikes;
                
                RestaurantDishes.Add(new DishWithStats 
                { 
                    Dish = dish, 
                    Likes = likes,
                    Dislikes = dislikes
                });
            }

            // Calculate engagement rate (likes + dislikes) / total dishes
            int totalInteractions = TotalLikes + TotalDislikes;
            EngagementRate = TotalDishes > 0 ? (int)((double)totalInteractions / TotalDishes * 100) : 0;
        }
    }
}
