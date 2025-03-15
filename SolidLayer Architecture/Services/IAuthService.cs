using SolidLayer_Architecture.Models;
using System.Security.Claims;

namespace SolidLayer_Architecture.Services
{
    public interface IAuthService
    {
        User? ValidateUser(string email, string password);
        ClaimsPrincipal CreateClaimsPrincipal(User user);
        Task SignIn(HttpContext httpContext, ClaimsPrincipal principal, bool isPersistent = false);
        Task SignOut(HttpContext httpContext);
    }
}
