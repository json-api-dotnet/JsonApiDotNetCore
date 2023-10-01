using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Used to expose a property on a resource class as a JSON:API attribute (https://jsonapi.org/format/#document-resource-object-attributes).
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public sealed class AttrAttribute : ResourceFieldAttribute
{
    private AttrCapabilities? _capabilities;

    internal bool HasExplicitCapabilities => _capabilities != null;

    /// <summary>
    /// The set of allowed capabilities on this attribute. When not explicitly set, the configured default set of capabilities is used.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// public class Author : Identifiable<long>
    /// {
    ///     [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort)]
    ///     public string Name { get; set; } = null!;
    /// }
    /// ]]></code>
    /// </example>
    public AttrCapabilities Capabilities
    {
        get => _capabilities ?? default;
        set => _capabilities = value;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        var other = (AttrAttribute)obj;

        return Capabilities == other.Capabilities && base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Capabilities, base.GetHashCode());
    }
}
