using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Swipe2TryCore.Models; // Use Swipe2TryCore models
using SolidLayer_Architecture.Services;
using System.ComponentModel.DataAnnotations;

namespace SolidLayer_Architecture.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IUserService userService, IAuthService authService, ILogger<RegisterModel> logger)
        {
            _userService = userService;
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new RegisterInputModel();

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public string ReturnUrl { get; set; } = string.Empty;

        public IList<Swipe2TryCore.Models.Role> AvailableRoles { get; set; } = new List<Swipe2TryCore.Models.Role>();

        public void OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;
            // Load available roles for the dropdown
            AvailableRoles = _userService.GetAllRoles().ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;
            AvailableRoles = _userService.GetAllRoles().ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (_userService.IsEmailTaken(Input.Email))
            {
                ErrorMessage = $"Email '{Input.Email}' is already taken.";
                return Page();
            }

            try
            {
                // Create the user with regular user role by default
                var user = new Swipe2TryCore.Models.User
                {
                    Name = Input.Name,
                    Email = Input.Email,
                    Password = Input.Password,
                    RoleID = "r3" // Regular user role
                };

                _userService.CreateUser(user);
                
                // Sign in the newly registered user
                var registeredUser = _userService.GetUserByEmail(Input.Email);
                if (registeredUser != null)
                {
                    var principal = _authService.CreateClaimsPrincipal(registeredUser);
                    await _authService.SignIn(HttpContext, principal);
                    
                    _logger.LogInformation("User {Email} registered successfully and signed in", Input.Email);

                    // Redirect to the return URL
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ErrorMessage = "An error occurred while trying to sign in.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration: {Email}", Input.Email);
                ErrorMessage = $"Registration failed: {ex.Message}";
                return Page();
            }
        }
    }

    public class RegisterInputModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
