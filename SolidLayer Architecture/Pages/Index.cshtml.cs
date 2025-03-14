using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SolidLayer_Architecture.Data;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Dish> Dishes { get; set; } // Property to hold the list of dishes

        public async Task OnGetAsync()
        {
            Dishes = await _context.Dishes.ToListAsync(); // Fetch all dishes
        }

    }
}
