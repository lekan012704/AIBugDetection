using Infrastructure.Authentication.Custom.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        //We definitely want to reject unauthenticated users here.
        //if (context.User is not { Identity.IsAuthenticated: true } or { Identity.IsAuthenticated: false })
        //if (!context.User?.Identity?.IsAuthenticated ?? true)
        //{
        //    context.Fail();
        //    return;
        //}

        //We definitely want to reject unauthenticated users here.
        if (!context.User?.Identity?.IsAuthenticated ?? true)
        {
            // If we're in an HTTP context, set the response
            if (context.Resource is HttpContext httpContext)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    Status = 401,
                    Message = "Authentication required. Please log in to access this resource.",
                    Error = "Unauthorized"
                });
            }

            return;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();

        //Guid userId = context.User.GetUserId();
        //HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId);

        string identityId = context.User.GetIdentityId();
        HashSet<string> permissions = await permissionProvider.GetPermissionsForUserAsync(identityId);

        //ErrorOr<HashSet<string>> permissions = await permissionProvider.GetPermissionsForUserAsync(identityId);

        //if (permissions.IsError)
        //{
        //    context.Fail();
        //    return;
        //}
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);

            return;
        }
    }
}
