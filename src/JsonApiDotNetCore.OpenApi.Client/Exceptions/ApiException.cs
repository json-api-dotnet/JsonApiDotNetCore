using JetBrains.Annotations;

// We cannot rely on generating ApiException as soon as we are generating multiple clients, see https://github.com/RicoSuter/NSwag/issues/2839#issuecomment-776647377.
// Instead, we configure NSwag to point to the exception below in the generated code.

namespace JsonApiDotNetCore.OpenApi.Client.Exceptions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class ApiException(string message, int statusCode, string? response, IReadOnlyDictionary<string, IEnumerable<string>> headers, Exception? innerException)
    : Exception($"HTTP {statusCode}: {message}", innerException)
{
    public int StatusCode { get; } = statusCode;
    public virtual string? Response { get; } = string.IsNullOrEmpty(response) ? null : response;
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; } = headers;
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ApiException<TResult>(
    string message, int statusCode, string? response, IReadOnlyDictionary<string, IEnumerable<string>> headers, TResult result, Exception? innerException)
    : ApiException(message, statusCode, response, headers, innerException)
{
    public TResult Result { get; } = result;
    public override string Response => $"The response body is unavailable. Use the {nameof(Result)} property instead.";
}
