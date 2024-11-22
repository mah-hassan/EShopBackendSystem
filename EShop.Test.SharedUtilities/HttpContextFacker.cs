using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace EShop.Test.SharedUtilities;

public static class HttpContextMockProvider
{
    public static HttpContext GetHttpContext(Guid? userId = null, string? email = null)
    {
        userId = userId ?? Guid.NewGuid();
        email = email ?? "test@example.com";
        var claims = new List<Claim> 
        { 
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()!),
            new Claim(ClaimTypes.Email, email) 
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        var ctxMock = new Mock<HttpContext>();
        ctxMock.Setup(x => x.User).Returns(principal);
        return ctxMock.Object;
    }
}