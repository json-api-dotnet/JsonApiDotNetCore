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

        public IReadOnlyList<ErrorObject> Errors { get; }

        public JsonApiException(ErrorObject error, Exception? innerException = null)
            : base(null, innerException)
        {
            ArgumentGuard.NotNull(error, nameof(error));

            Errors = error.AsArray();
        }

        public JsonApiException(IEnumerable<ErrorObject> errors, Exception? innerException = null)
            : base(null, innerException)
        {
            IReadOnlyList<ErrorObject>? errorList = ToErrorList(errors);
            ArgumentGuard.NotNullNorEmpty(errorList, nameof(errors));

            Errors = errorList;
        }

        private static IReadOnlyList<ErrorObject>? ToErrorList(IEnumerable<ErrorObject>? errors)
        {
            return errors?.ToList();
        }

        public string GetSummary()
        {
            return $"{nameof(JsonApiException)}: Errors = {JsonSerializer.Serialize(Errors, SerializerOptions)}";
        }
    }
}
