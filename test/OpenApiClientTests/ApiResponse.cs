using JsonApiDotNetCore.OpenApi.Client;
using JsonApiDotNetCore.OpenApi.Client.Exceptions;

#pragma warning disable AV1008 // Class should not be static

namespace OpenApiClientTests;

internal static class ApiResponse
{
    public static async Task<TResponse?> TranslateAsync<TResponse>(Func<Task<TResponse>> operation)
        where TResponse : class
    {
        ArgumentGuard.NotNull(operation);

        try
        {
            return await operation();
        }
        catch (ApiException exception) when (exception.StatusCode == 204)
        {
            // Workaround for https://github.com/RicoSuter/NSwag/issues/2499
            return null;
        }
    }
}
