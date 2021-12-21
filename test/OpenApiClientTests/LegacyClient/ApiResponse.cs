using JsonApiDotNetCore.OpenApi.Client;
using JsonApiDotNetCore.OpenApi.Client.Exceptions;

#pragma warning disable AV1008 // Class should not be static

namespace OpenApiClientTests.LegacyClient;

internal static class ApiResponse
{
    public static async Task<TResponse?> TranslateAsync<TResponse>(Func<Task<TResponse>> operation)
        where TResponse : class
    {
        // Workaround for https://github.com/RicoSuter/NSwag/issues/2499

        ArgumentGuard.NotNull(operation, nameof(operation));

        try
        {
            return await operation();
        }
        catch (ApiException exception)
        {
            if (exception.StatusCode != 204)
            {
                throw;
            }

            return null;
        }
    }
}
