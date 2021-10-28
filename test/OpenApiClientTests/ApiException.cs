using System;
using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace OpenApiClientTests.Exceptions
{
    // We cannot rely on a generated ApiException as soon as we are generating multiple clients, see https://github.com/RicoSuter/NSwag/issues/2839#issuecomment-776647377.
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal class ApiException : Exception
    {
        public int StatusCode { get; }

        public string Response { get; }

        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

        public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, Exception innerException)
            : base(
                message + "\n\nStatus: " + statusCode + "\nResponse: \n" +
                (response == null ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
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

        public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, TResult result,
            Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }
}
