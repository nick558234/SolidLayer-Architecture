using System.Security.Claims;

namespace SolidLayer_Architecture.Services
{
    public interface IAuthService
    {
        Swipe2TryCore.Models.User? ValidateUser(string email, string password);
        ClaimsPrincipal CreateClaimsPrincipal(Swipe2TryCore.Models.User user);
        Task SignIn(HttpContext httpContext, ClaimsPrincipal principal, bool isPersistent = false);
        Task SignOut(HttpContext httpContext);
    }
}
