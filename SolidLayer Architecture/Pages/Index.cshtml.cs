using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IDishService _dishService;

        public IndexModel(IDishService dishService)
        {
            _dishService = dishService;
        }

        public IList<Dish> Dishes { get; set; } = new List<Dish>();

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
        }
    }
}
