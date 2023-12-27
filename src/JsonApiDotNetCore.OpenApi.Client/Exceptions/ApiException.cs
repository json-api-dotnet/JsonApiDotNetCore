using JetBrains.Annotations;

// We cannot rely on generating ApiException as soon as we are generating multiple clients, see https://github.com/RicoSuter/NSwag/issues/2839#issuecomment-776647377.
// Instead, we configure NSwag to point to the exception below in the generated code.

namespace JsonApiDotNetCore.OpenApi.Client.Exceptions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ApiException(
    string message, int statusCode, string? response, IReadOnlyDictionary<string, IEnumerable<string>> headers, Exception? innerException)
    : Exception($"{message}\n\nStatus: {statusCode}\nResponse: \n{response ?? "(null)"}", innerException)
{
    public int StatusCode { get; } = statusCode;
    public string? Response { get; } = response;
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; } = headers;
}
