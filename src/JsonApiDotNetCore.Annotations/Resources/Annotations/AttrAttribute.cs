using System.Collections.ObjectModel;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Used to expose a property on a resource class as a JSON:API attribute (https://jsonapi.org/format/#document-resource-object-attributes).
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public sealed class AttrAttribute : ResourceFieldAttribute, IFieldContainer
{
    private static readonly ReadOnlyDictionary<string, AttrAttribute> EmptyChildren = new Dictionary<string, AttrAttribute>().AsReadOnly();

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
    /// Indicates whether this attribute contains nested attributes. <c>false</c> by default.
    /// </summary>
    public bool IsCompound { get; set; }

    /// <summary>
    /// Gets the kind of this attribute.
    /// </summary>
    public AttrKind Kind { get; internal set; }

    /// <summary>
    /// Gets the nested attributes by name, if this is a compound attribute.
    /// </summary>
    public IReadOnlyDictionary<string, AttrAttribute> Children { get; internal set; } = EmptyChildren;

    /// <inheritdoc />
    public Type ClrType => Property.PropertyType;

    /// <inheritdoc />
    public AttrAttribute? FindAttributeByPublicName(string publicName)
    {
        return Children.GetValueOrDefault(publicName);
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

        return Capabilities == other.Capabilities && Kind == other.Kind && Children.DictionaryEqual(other.Children) && base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return HashCode.Combine(Capabilities, Kind, Children, base.GetHashCode());
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}
