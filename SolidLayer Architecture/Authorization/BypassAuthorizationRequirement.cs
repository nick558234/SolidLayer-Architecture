using Microsoft.AspNetCore.Authorization;

namespace SolidLayer_Architecture.Authorization
{
    public class BypassAuthorizationRequirement : IAuthorizationRequirement
    {
        // This is just a marker interface for the requirement
    }

    public class BypassAuthorizationHandler : AuthorizationHandler<BypassAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            BypassAuthorizationRequirement requirement)
        {
            var httpContext = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
            
            if (httpContext != null)
            {
                string path = httpContext.Request.Path;
                if (path.StartsWith("/Setup") || 
                    path.StartsWith("/AccessHelp") || 
                    path.StartsWith("/Admin/AdminDiagnostics") || 
                    path.StartsWith("/Admin/UserDiagnostics"))
                {
                    // Always succeed for these paths
                    context.Succeed(requirement);
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
