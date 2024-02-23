using System.Net;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Client;

[PublicAPI]
public class ApiResponse(int statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers)
{
    public int StatusCode { get; private set; } = statusCode;
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; } = headers;

    public static async Task<TResponse?> TranslateAsync<TResponse>(Func<Task<TResponse>> operation)
        where TResponse : class
    {
        ArgumentGuard.NotNull(operation);

        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (ApiException exception) when (exception.StatusCode is (int)HttpStatusCode.NoContent or (int)HttpStatusCode.NotModified)
        {
            // Workaround for https://github.com/RicoSuter/NSwag/issues/2499
            return null;
        }
    }

    public static async Task TranslateAsync(Func<Task> operation)
    {
        ArgumentGuard.NotNull(operation);

        try
        {
            await operation().ConfigureAwait(false);
        }
        catch (ApiException exception) when (exception.StatusCode is (int)HttpStatusCode.NoContent or (int)HttpStatusCode.NotModified)
        {
            // Workaround for https://github.com/RicoSuter/NSwag/issues/2499
        }
    }

    public static async Task<ApiResponse<TResult?>> TranslateAsync<TResult>(Func<Task<ApiResponse<TResult>>> operation)
        where TResult : class
    {
        ArgumentGuard.NotNull(operation);

        try
        {
            return (await operation().ConfigureAwait(false))!;
        }
        catch (ApiException exception) when (exception.StatusCode is (int)HttpStatusCode.NoContent or (int)HttpStatusCode.NotModified)
        {
            // Workaround for https://github.com/RicoSuter/NSwag/issues/2499
            return new ApiResponse<TResult?>(exception.StatusCode, exception.Headers, null);
        }
    }

    public static async Task<ApiResponse> TranslateAsync(Func<Task<ApiResponse>> operation)
    {
        ArgumentGuard.NotNull(operation);

        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (ApiException exception) when (exception.StatusCode is (int)HttpStatusCode.NoContent or (int)HttpStatusCode.NotModified)
        {
            // Workaround for https://github.com/RicoSuter/NSwag/issues/2499
            return new ApiResponse(exception.StatusCode, exception.Headers);
        }
    }
}

[PublicAPI]
public class ApiResponse<TResult>(int statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers, TResult result) : ApiResponse(statusCode, headers)
{
    public TResult Result { get; private set; } = result;
}
