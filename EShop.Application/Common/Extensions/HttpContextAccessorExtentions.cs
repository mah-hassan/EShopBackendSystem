using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EShop.Application.Common.Extensions;

internal static class HttpContextAccessorExtentions
{
    public static Guid GetUserId(this IHttpContextAccessor httpContextAccessor)
    {
        string? userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException();
        }
        return Guid.Parse(userId);
    }
    public static string GetUserEmail(this IHttpContextAccessor httpContextAccessor)
    {
        string? userEmail = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            throw new InvalidOperationException();
        }
        return userEmail;
    }
}