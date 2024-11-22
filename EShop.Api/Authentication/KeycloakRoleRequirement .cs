using Microsoft.AspNetCore.Authorization;

namespace EShop.Api.Authentication;

public sealed class KeycloakRoleRequirement : IAuthorizationRequirement
{
    public string ClientId { get; }
    public string RequiredRole { get; }

    public KeycloakRoleRequirement(string clientId, string requiredRole)
    {
        ClientId = clientId;
        RequiredRole = requiredRole;
    }
}