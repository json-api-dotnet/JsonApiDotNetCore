using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Metadata about the shape of a compound JSON:API attribute in the resource graph.
/// </summary>
[PublicAPI]
public sealed class CompoundType : IAttributeContainer
{
    private readonly Dictionary<string, AttrAttribute> _attributesByPublicName = [];
    private readonly Dictionary<string, AttrAttribute> _attributesByPropertyName = [];

    /// <inheritdoc />
    public Type ClrType { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<AttrAttribute> Attributes { get; }

    public CompoundType(Type clrType, IReadOnlyCollection<AttrAttribute> attributes)
    {
        ArgumentNullException.ThrowIfNull(clrType);
        ArgumentNullException.ThrowIfNull(attributes);

        ClrType = clrType;
        Attributes = attributes;

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
