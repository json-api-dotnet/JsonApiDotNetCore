using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Used to expose a property on a resource class as a JSON:API attribute (https://jsonapi.org/format/#document-resource-object-attributes).
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public sealed class AttrAttribute : ResourceFieldAttribute
{
    private readonly Lazy<bool> _isCollectionLazy;
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
    /// Evaluates whether the underlying property is a collection.
    /// Non-settable; evaluated on demand from <see cref="ResourceFieldAttribute.Property"/>.
    /// </summary>
    public bool IsCollection => _isCollectionLazy.Value;

    /// <summary>
    /// The schema of the nested compound type when <see cref="IsCompound"/> is true; otherwise <c>null</c>.
    /// </summary>
    public CompoundType? CompoundType { get; internal set; }

    public AttrAttribute()
    {
        _isCollectionLazy = new Lazy<bool>(EvaluateIsCollection, LazyThreadSafetyMode.PublicationOnly);
    }

    private bool EvaluateIsCollection()
    {
        return Property.PropertyType != typeof(string) && CollectionConverter.Instance.FindCollectionElementType(Property.PropertyType) != null;
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

        return Capabilities == other.Capabilities && IsCompound == other.IsCompound && CompoundType == other.CompoundType && base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Capabilities, IsCompound, CompoundType, base.GetHashCode());
    }
}
