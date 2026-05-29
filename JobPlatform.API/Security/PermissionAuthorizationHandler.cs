using Microsoft.AspNetCore.Authorization;

namespace JobPlatform.API.Security;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var isAdmin = context.User.IsInRole("admin");
        var hasPermission = context.User.HasClaim("permission", requirement.Permission);

        if (isAdmin || hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
