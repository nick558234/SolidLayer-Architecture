using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.Dishes
{
    public class SwipeModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILogger<SwipeModel> _logger;

        public SwipeModel(IDishService dishService, ILogger<SwipeModel> logger)
        {
            _dishService = dishService;
            _logger = logger;
        }

        public Dish? CurrentDish { get; private set; }
        public bool HasMoreDishes { get; private set; }

        public IActionResult OnGet()
        {
            // Get all dishes
            var dishes = _dishService.GetAllDishes().ToList();

            // Check if there are any dishes
            if (dishes.Any())
            {
                // Get a random dish for swiping
                // In a real app, you would implement logic to avoid showing already swiped dishes
                Random random = new Random();
                int index = random.Next(dishes.Count);
                CurrentDish = dishes[index];
                HasMoreDishes = true;
            }
            else
            {
                HasMoreDishes = false;
            }

            return Page();
        }
    }
}
