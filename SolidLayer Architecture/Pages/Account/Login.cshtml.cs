using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Services;

namespace SolidLayer_Architecture.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new LoginInputModel();

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public string ReturnUrl { get; set; } = string.Empty;

        public void OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validate the user credentials
            var user = _authService.ValidateUser(Input.Email, Input.Password);
            
            if (user == null)
            {
                ErrorMessage = "Invalid login attempt.";
                _logger.LogWarning("Invalid login attempt for email: {Email}", Input.Email);
                return Page();
            }

            // Create claims principal for the authenticated user
            var principal = _authService.CreateClaimsPrincipal(user);
            
            // Sign in the user
            await _authService.SignIn(HttpContext, principal, Input.RememberMe);
            
            _logger.LogInformation("User {Email} logged in successfully", user.Email);

            // Redirect to the return URL
            return LocalRedirect(returnUrl);
        }
    }

    public class LoginInputModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
