using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILikeDislikeService _likeDislikeService;

        public IndexModel(IDishService dishService, ILikeDislikeService likeDislikeService)
        {
            _dishService = dishService;
            _likeDislikeService = likeDislikeService;
        }

        public IList<Dish> Dishes { get; set; } = new List<Dish>();
        public Dictionary<string, int> DishLikeCounts { get; set; } = new Dictionary<string, int>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Dishes = _dishService.SearchDishes(SearchTerm).ToList();
            }
            else
            {
                Dishes = _dishService.GetAllDishes().ToList();
            }

            // Get like counts for each dish
            foreach (var dish in Dishes)
            {
                DishLikeCounts[dish.DishID] = _likeDislikeService.GetLikeCount(dish.DishID);
            }
        }
    }
}
