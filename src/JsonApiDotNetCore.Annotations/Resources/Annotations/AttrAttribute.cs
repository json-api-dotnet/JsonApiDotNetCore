using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Used to expose a property on a resource class as a JSON:API attribute (https://jsonapi.org/format/#document-resource-object-attributes).
/// </summary>
/// <remarks>
/// Note: System.Text.Json attributes (such as [JsonPropertyName] and [JsonIgnore]) are not supported on JSON:API resource and compound models and are
/// never taken into account when reading request bodies, rendering responses, or generating error source pointers. Schema names and capabilities are
/// governed exclusively by [Attr(PublicName)] and [Attr(Capabilities)].
/// </remarks>
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

    /// <summary>
    /// Indicates whether this attribute is a complex/compound object. Defaults to <c>false</c>.
    /// </summary>
    public bool IsCompound { get; set; }

    /// <summary>
    /// Indicates whether the underlying property is a collection.
    /// </summary>
    public bool IsCollection { get; internal set; }

    /// <summary>
    /// The schema of the nested compound type when <see cref="IsCompound" /> is true; otherwise <c>null</c>.
    /// </summary>
    public CompoundType? CompoundType { get; internal set; }

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

        return Capabilities == other.Capabilities && IsCompound == other.IsCompound && CompoundType == other.CompoundType && base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Capabilities, IsCompound, CompoundType, base.GetHashCode());
    }
}
