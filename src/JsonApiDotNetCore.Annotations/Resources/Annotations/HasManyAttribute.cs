using JetBrains.Annotations;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Used to expose a property on a resource class as a JSON:API to-many relationship
/// (https://jsonapi.org/format/#document-resource-object-relationships).
/// </summary>
/// <example>
/// <code><![CDATA[
/// public class Author : Identifiable
/// {
///     [HasMany]
///     public ISet<Article> Articles { get; set; }
/// }
/// ]]></code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public sealed class HasManyAttribute : RelationshipAttribute
{
    private readonly Lazy<bool> _lazyIsManyToMany;
    private HasManyCapabilities? _capabilities;

    /// <summary>
    /// Inspects <see cref="RelationshipAttribute.InverseNavigationProperty" /> to determine if this is a many-to-many relationship.
    /// </summary>
    internal bool IsManyToMany => _lazyIsManyToMany.Value;

    internal bool HasExplicitCapabilities => _capabilities != null;

    /// <summary>
    /// The set of allowed capabilities on this to-many relationship. When not explicitly set, the configured default set of capabilities is used.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// public class Book : Identifiable<long>
    /// {
    ///     [HasMany(Capabilities = HasManyCapabilities.AllowView | HasManyCapabilities.AllowInclude)]
    ///     public ISet<Chapter> Chapters { get; set; } = new HashSet<Chapter>();
    /// }
    /// ]]></code>
    /// </example>
    public HasManyCapabilities Capabilities
    {
        get => _capabilities ?? default;
        set => _capabilities = value;
    }

    public HasManyAttribute()
    {
        _lazyIsManyToMany = new Lazy<bool>(EvaluateIsManyToMany, LazyThreadSafetyMode.PublicationOnly);
    }

    private bool EvaluateIsManyToMany()
    {
        if (InverseNavigationProperty != null)
        {
            Type? elementType = CollectionConverter.FindCollectionElementType(InverseNavigationProperty.PropertyType);
            return elementType != null;
        }

        return false;
    }

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

        var other = (HasManyAttribute)obj;

        return _capabilities == other._capabilities && base.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_capabilities, base.GetHashCode());
    }
}
