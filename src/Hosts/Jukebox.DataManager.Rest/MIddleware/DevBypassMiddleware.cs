using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace Jukebox.DataManager.Rest.Middleware;

public class DevBypassMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _enabled;

    public DevBypassMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _enabled = configuration.GetValue<bool>("Auth:DevBypass");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_enabled)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user"),
                new Claim("sub", "dev-user")
            };
            var identity = new ClaimsIdentity(claims, authenticationType: "DevBypass");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}