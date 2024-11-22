using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace EShop.Api.Authentication;

public sealed class KeycloakRoleHandler(ILogger<KeycloakRoleHandler> logger) : AuthorizationHandler<KeycloakRoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, KeycloakRoleRequirement requirement)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // Find the roles under "resource_access" for the specified client
        var resourceAccess = user.FindFirst("resource_access")?.Value;
        if (resourceAccess != null)
        {
            var parsedResourceAccess = JsonDocument.Parse(resourceAccess);
            if (parsedResourceAccess.RootElement.TryGetProperty(requirement.ClientId, out var client))
            {
                if (client.TryGetProperty("roles", out var roles))
                {
                    if (roles.EnumerateArray().Any(role => role.GetString() == requirement.RequiredRole))
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail(new AuthorizationFailureReason(this, $"Missing required role {requirement.RequiredRole}"));
                        logger.LogInformation("Missing required role: '{requirement.RequiredRole}'", requirement.RequiredRole);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}