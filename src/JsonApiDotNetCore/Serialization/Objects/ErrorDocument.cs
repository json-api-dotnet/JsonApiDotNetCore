using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public sealed class ErrorDocument
    {
        public IReadOnlyList<Error> Errors { get; }

        public ErrorDocument()
            : this(Array.Empty<Error>())
        {
        }

        public ErrorDocument(Error error)
            : this(new[] {error})
        {
        }

        public ErrorDocument(IEnumerable<Error> errors)
        {
            ArgumentGuard.NotNull(errors, nameof(errors));

            Errors = errors.ToList();
        }

        public HttpStatusCode GetErrorStatusCode()
        {
            var statusCodes = Errors
                .Select(e => (int)e.StatusCode)
                .Distinct()
                .ToArray();

            if (statusCodes.Length == 1)
                return (HttpStatusCode)statusCodes[0];

            var statusCode = int.Parse(statusCodes.Max().ToString()[0] + "00");
            return (HttpStatusCode)statusCode;
        }
    }
}
