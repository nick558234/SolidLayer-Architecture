using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;

namespace SolidLayer_Architecture.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IAuthService authService, ILogger<LogoutModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Sign out the user
            await _authService.SignOut(HttpContext);
            _logger.LogInformation("User logged out");
            
            // Redirect to the homepage
            return RedirectToPage("/Index");
        }
    }
}
