using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <summary>
/// The containing type for a JSON:API field, which can be a <see cref="ResourceType" /> or <see cref="AttrAttribute" />.
/// </summary>
internal sealed class FieldContainer
{
    public ResourceType? Type { get; }
    public AttrAttribute? Attribute { get; }

    public string PublicName => Type != null ? Type.PublicName : Attribute!.PublicName;
    public Type ClrType => Type != null ? Type.ClrType : Attribute!.Property.PropertyType;

    public FieldContainer(ResourceType? type, AttrAttribute? attribute)
    {
        AssertSingleSpecified(type, attribute);

        Type = type;
        Attribute = attribute;
    }

    private static void AssertSingleSpecified(ResourceType? type, AttrAttribute? attribute)
    {
        if (type == null && attribute == null)
        {
            throw new ArgumentException("Resource type or attribute must be specified.");
        }

        if (type != null && attribute != null)
        {
            throw new ArgumentException("Either resource type or attribute must be specified, not both.");
        }
    }

    public AttrAttribute? FindAttributeByPublicName(string publicName)
    {
        ArgumentNullException.ThrowIfNull(publicName);

        if (Type != null)
        {
            return Type.FindAttributeByPublicName(publicName);
        }

        if (Attribute != null)
        {
            if (Attribute.Children.TryGetValue(publicName, out AttrAttribute? childAttribute))
            {
                return childAttribute;
            }
        }

        return null;
    }

    public override string ToString()
    {
        return Type != null ? $"resource type '{Type}'" : Attribute != null ? $"type '{Attribute.Property.PropertyType}'" : string.Empty;
    }
}
