using Jukebox.DataManager.Rest.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Jukebox.DataManager.Rest.Test;

public class DevBypassMiddlewareTests
{
    private static DevBypassMiddleware CreateMiddleware(bool enabled, RequestDelegate next)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:DevBypass"] = enabled.ToString()
            })
            .Build();

        return new DevBypassMiddleware(next, config);
    }

    [Fact]
    public async Task InvokeAsync_BypassEnabled_SetsDevUserPrincipal()
    {
        var middleware = CreateMiddleware(enabled: true, next: _ => Task.CompletedTask);
        var httpContext = new DefaultHttpContext();

        await middleware.InvokeAsync(httpContext);

        Assert.NotNull(httpContext.User);
        Assert.Equal("dev-user", httpContext.User.FindFirstValue("sub"));
    }

    [Fact]
    public async Task InvokeAsync_BypassDisabled_DoesNotSetPrincipal()
    {
        var middleware = CreateMiddleware(enabled: false, next: _ => Task.CompletedTask);
        var httpContext = new DefaultHttpContext();

        await middleware.InvokeAsync(httpContext);

        Assert.Null(httpContext.User.FindFirstValue("sub"));
    }

    [Fact]
    public async Task InvokeAsync_BypassEnabled_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(enabled: true, next: _ => { nextCalled = true; return Task.CompletedTask; });
        var httpContext = new DefaultHttpContext();

        await middleware.InvokeAsync(httpContext);

        Assert.True(nextCalled);
    }
}