using Grpc.Core;
using Grpc.Core.Testing;
using Jukebox.DataManager.Grpc.Interceptors;
using Microsoft.Extensions.Configuration;

namespace Jukebox.DataManager.Grpc.Test;

public class AuthInterceptorTests
{
    private readonly AuthInterceptor _interceptor;

    public AuthInterceptorTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKeys:service-a"] = "valid-key-123"
            })
            .Build();

        _interceptor = new AuthInterceptor(config);
    }

    private static ServerCallContext CreateContext(string? apiKey = null)
    {
        var headers = new Metadata();
        if (apiKey is not null)
            headers.Add("x-api-key", apiKey);

        return TestServerCallContext.Create(
            method: "TestMethod",
            host: "localhost",
            deadline: DateTime.MaxValue,
            requestHeaders: headers,
            cancellationToken: CancellationToken.None,
            peer: "127.0.0.1",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: _ => Task.CompletedTask,
            writeOptionsGetter: () => null,
            writeOptionsSetter: _ => { });
    }

    [Fact]
    public async Task UnaryServerHandler_ValidKey_SetsUserIdAndContinues()
    {
        var context = CreateContext("valid-key-123");
        var called = false;

        await _interceptor.UnaryServerHandler<string, string>(
            "request",
            context,
            (_, _) => { called = true; return Task.FromResult("response"); });

        Assert.True(called);
        Assert.Equal("service-a", context.UserState["userId"]);
    }

    [Fact]
    public async Task UnaryServerHandler_InvalidKey_ThrowsUnauthenticated()
    {
        var context = CreateContext("wrong-key");

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            _interceptor.UnaryServerHandler<string, string>(
                "request",
                context,
                (_, _) => Task.FromResult("response")));

        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task UnaryServerHandler_MissingKey_ThrowsUnauthenticated()
    {
        var context = CreateContext();

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            _interceptor.UnaryServerHandler<string, string>(
                "request",
                context,
                (_, _) => Task.FromResult("response")));

        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task UnaryServerHandler_NoKeysConfigured_ThrowsUnauthenticated()
    {
        var emptyConfig = new ConfigurationBuilder().Build();
        var interceptor = new AuthInterceptor(emptyConfig);
        var context = CreateContext("valid-key-123");

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            interceptor.UnaryServerHandler<string, string>(
                "request",
                context,
                (_, _) => Task.FromResult("response")));

        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }
}