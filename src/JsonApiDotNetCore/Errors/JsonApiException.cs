using System.Collections.ObjectModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors;

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
        ArgumentNullException.ThrowIfNull(error);

        Errors = [error];
    }

    public JsonApiException(IEnumerable<ErrorObject> errors, Exception? innerException = null)
        : base(null, innerException)
    {
        ReadOnlyCollection<ErrorObject>? errorCollection = ToCollection(errors);
        ArgumentGuard.NotNullNorEmpty(errorCollection, nameof(errors));

        Errors = errorCollection;
    }

    private static ReadOnlyCollection<ErrorObject>? ToCollection(IEnumerable<ErrorObject>? errors)
    {
        return errors?.ToArray().AsReadOnly();
    }

    public string GetSummary()
    {
        return $"{nameof(JsonApiException)}: Errors = {JsonSerializer.Serialize(Errors, SerializerOptions)}";
    }
}
