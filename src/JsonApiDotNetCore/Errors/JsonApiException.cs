using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The base class for an <see cref="Exception" /> that represents one or more JSON:API error objects in an unsuccessful response.
    /// </summary>
    [PublicAPI]
    public class JsonApiException : Exception
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public IReadOnlyList<Error> Errors { get; }

        public override string Message => $"Errors = {JsonSerializer.Serialize(Errors, SerializerOptions)}";

        public JsonApiException(Error error, Exception innerException = null)
            : base(null, innerException)
        {
            ArgumentGuard.NotNull(error, nameof(error));

            Errors = error.AsArray();
        }

        public JsonApiException(IEnumerable<Error> errors, Exception innerException = null)
            : base(null, innerException)
        {
            List<Error> errorList = errors?.ToList();
            ArgumentGuard.NotNullNorEmpty(errorList, nameof(errors));

            Errors = errorList;
        }
    }
}
