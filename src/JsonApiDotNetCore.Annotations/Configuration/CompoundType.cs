using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Metadata about the shape of an embedded compound value object in the resource graph.
/// </summary>
/// <remarks>
/// <para>
/// Compound types used with sparse fieldsets or request deserialization must provide an accessible parameterless constructor (can be non-public) to
/// support member initialization expressions and object instantiation.
/// </para>
/// <para>
/// Note: System.Text.Json attributes (such as [JsonPropertyName] and [JsonIgnore]) are not supported on JSON:API resource and compound models and are
/// never taken into account when reading request bodies, rendering responses, or generating error source pointers. Schema names and capabilities are
/// governed exclusively by [Attr(PublicName)] and [Attr(Capabilities)].
/// </para>
/// </remarks>
[PublicAPI]
public sealed class CompoundType : IAttributeContainer
{
    private readonly Dictionary<string, AttrAttribute> _attributesByPublicName = [];
    private readonly Dictionary<string, AttrAttribute> _attributesByPropertyName = [];

    /// <inheritdoc />
    public Type ClrType { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<AttrAttribute> Attributes { get; }

    public CompoundType(Type clrType, IReadOnlyCollection<AttrAttribute>? attributes = null)
    {
        ArgumentNullException.ThrowIfNull(clrType);

        ClrType = clrType;
        Attributes = attributes ?? [];

        foreach (AttrAttribute attribute in Attributes)
        {
            _attributesByPublicName.Add(attribute.PublicName, attribute);
            _attributesByPropertyName.Add(attribute.Property.Name, attribute);
        }
    }

    /// <inheritdoc />
    public AttrAttribute GetAttributeByPublicName(string publicName)
    {
        AttrAttribute? attribute = FindAttributeByPublicName(publicName);
        return attribute ?? throw new InvalidOperationException($"Attribute '{publicName}' does not exist on compound type '{ClrType.Name}'.");
    }

    /// <inheritdoc />
    public AttrAttribute? FindAttributeByPublicName(string publicName)
    {
        ArgumentNullException.ThrowIfNull(publicName);

        return _attributesByPublicName.GetValueOrDefault(publicName);
    }

    /// <inheritdoc />
    public AttrAttribute GetAttributeByPropertyName(string propertyName)
    {
        AttrAttribute? attribute = FindAttributeByPropertyName(propertyName);
        return attribute ?? throw new InvalidOperationException($"Attribute for property '{propertyName}' does not exist on compound type '{ClrType.Name}'.");
    }

    /// <inheritdoc />
    public AttrAttribute? FindAttributeByPropertyName(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        return _attributesByPropertyName.GetValueOrDefault(propertyName);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ClrType.Name;
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

        var other = (CompoundType)obj;
        return ClrType == other.ClrType && Attributes.SequenceEqual(other.Attributes);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(ClrType);

        foreach (AttrAttribute attribute in Attributes)
        {
            hashCode.Add(attribute);
        }

        return hashCode.ToHashCode();
    }
}
