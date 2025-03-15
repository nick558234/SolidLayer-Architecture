using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SolidLayer_Architecture.Models;
using SolidLayer_Architecture.Services;

namespace SolidLayer_Architecture.Pages.Admin
{
    [Authorize(Policy = "RequireAdminRole")]
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersModel> _logger;

        public UsersModel(IUserService userService, ILogger<UsersModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public IList<SolidLayer_Architecture.Models.User> Users { get; set; } = new List<SolidLayer_Architecture.Models.User>();
        public SelectList Roles { get; set; } = new SelectList(Enumerable.Empty<Role>(), "RoleID", "RoleName");

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        [BindProperty]
        public UserEditModel EditingUser { get; set; } = new UserEditModel();

        public void OnGet()
        {
            Users = _userService.GetAllUsers().ToList();
            
            var roles = _userService.GetAllRoles().ToList();
            Roles = new SelectList(roles, "RoleID", "RoleName");
        }

        public IActionResult OnPostEdit()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors.";
                return RedirectToPage();
            }

            try
            {
                var user = _userService.GetUserById(EditingUser.UserID);
                
                if (user == null)
                {
                    ErrorMessage = "User not found.";
                    return RedirectToPage();
                }

                // Update only the role
                user.RoleID = EditingUser.RoleID;
                _userService.UpdateUser(user);
                
                StatusMessage = $"User {user.Name} role has been updated.";
                _logger.LogInformation("Admin updated role for user {UserID} to role {RoleID}", user.UserID, user.RoleID);
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role: {UserID}", EditingUser.UserID);
                ErrorMessage = $"Error updating user: {ex.Message}";
                return RedirectToPage();
            }
        }

        public class UserEditModel
        {
            public string UserID { get; set; } = string.Empty;
            public string RoleID { get; set; } = string.Empty;
        }
    }
}
