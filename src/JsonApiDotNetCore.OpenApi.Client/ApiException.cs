using JetBrains.Annotations;

// We cannot rely on generating ApiException as soon as we are generating multiple clients, see https://github.com/RicoSuter/NSwag/issues/2839#issuecomment-776647377.
// Instead, we configure NSwag to point to the exception below in the generated code.

// ReSharper disable once CheckNamespace
namespace JsonApiDotNetCore.OpenApi.Client.Exceptions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class ApiException : Exception
{
    public int StatusCode { get; }

    public string? Response { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    public ApiException(string message, int statusCode, string? response, IReadOnlyDictionary<string, IEnumerable<string>> headers, Exception innerException)
        : base(
            message + "\n\nStatus: " + statusCode + "\nResponse: \n" +
            (response == null ? "(null)" : response[..(response.Length >= 512 ? 512 : response.Length)]), innerException)
    {
        StatusCode = statusCode;
        Response = response;
        Headers = headers;
    }

    public override string ToString()
    {
        return $"HTTP Response: \n\n{Response}\n\n{base.ToString()}";
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ApiException<TResult> : ApiException
{
    public TResult Result { get; }

    public ApiException(string message, int statusCode, string? response, IReadOnlyDictionary<string, IEnumerable<string>> headers, TResult result,
        Exception innerException)
        : base(message, statusCode, response, headers, innerException)
    {
        Result = result;
    }
}
