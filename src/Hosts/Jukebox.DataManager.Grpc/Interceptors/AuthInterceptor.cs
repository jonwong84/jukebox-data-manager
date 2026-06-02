using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Jukebox.DataManager.Grpc.Interceptors;

public class AuthInterceptor : Interceptor
{
    private readonly IConfiguration _configuration;

    private const string InvalidApiKeyMessage = "Invalid or missing API key.";
    private const string UserIdKey = "userId";

    public AuthInterceptor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private bool TryAuthenticate(ServerCallContext context, out string userId)
    {
        userId = string.Empty;

        var apiKeys = _configuration.GetSection("ApiKeys").Get<Dictionary<string, string>>();
        if (apiKeys is null || apiKeys.Count == 0)
            return false;

        var keyEntry = context.RequestHeaders.FirstOrDefault(h => h.Key == "x-api-key");
        if (keyEntry is null)
            return false;

        var match = apiKeys.FirstOrDefault(kvp => kvp.Value == keyEntry.Value);
        if (match.Key is null)
            return false;

        userId = match.Key;
        return true;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (!TryAuthenticate(context, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, InvalidApiKeyMessage));

        context.UserState[UserIdKey] = userId;
        return await continuation(request, context);
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        if (!TryAuthenticate(context, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, InvalidApiKeyMessage));

        context.UserState[UserIdKey] = userId;
        return await continuation(requestStream, context);
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        if (!TryAuthenticate(context, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, InvalidApiKeyMessage));

        context.UserState[UserIdKey] = userId;
        await continuation(request, responseStream, context);
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        if (!TryAuthenticate(context, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, InvalidApiKeyMessage));

        context.UserState[UserIdKey] = userId;
        await continuation(requestStream, responseStream, context);
    }
}