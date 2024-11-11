using JetBrains.Annotations;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// Represents a JSON:API extension (in unescaped format), which occurs as an "ext" parameter inside an HTTP Accept or Content-Type header.
/// </summary>
[PublicAPI]
public sealed class JsonApiMediaTypeExtension : IEquatable<JsonApiMediaTypeExtension>
{
    public static readonly JsonApiMediaTypeExtension AtomicOperations = new("https://jsonapi.org/ext/atomic");
    public static readonly JsonApiMediaTypeExtension RelaxedAtomicOperations = new("atomic-operations");

    public string UnescapedValue { get; }

    public JsonApiMediaTypeExtension(string unescapedValue)
    {
        ArgumentGuard.NotNullNorEmpty(unescapedValue);

        UnescapedValue = unescapedValue;
    }

    public override string ToString()
    {
        return UnescapedValue;
    }

    public bool Equals(JsonApiMediaTypeExtension? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return UnescapedValue == other.UnescapedValue;
    }

    public override bool Equals(object? other)
    {
        return Equals(other as JsonApiMediaTypeExtension);
    }

    public override int GetHashCode()
    {
        return UnescapedValue.GetHashCode();
    }
}
