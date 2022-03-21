using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Used to expose a property on a resource class as a JSON:API relationship (https://jsonapi.org/format/#document-resource-object-relationships).
/// </summary>
[PublicAPI]
public abstract class RelationshipAttribute : ResourceFieldAttribute
{
    private protected static readonly CollectionConverter CollectionConverter = new();

    // These are definitely assigned after building the resource graph, which is why their public equivalents are declared as non-nullable.
    private ResourceType? _rightType;

    /// <summary>
    /// The <see cref="PropertyInfo" /> of the Entity Framework Core inverse navigation, which may or may not exist. Even if it exists, it may not be exposed
    /// as a JSON:API relationship.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// public class Article : Identifiable
    /// {
    ///     [HasOne] // InverseNavigationProperty: Person.Articles
    ///     public Person Owner { get; set; }
    /// }
    /// 
    /// public class Person : Identifiable
    /// {
    ///     [HasMany] // InverseNavigationProperty: Article.Owner
    ///     public ICollection<Article> Articles { get; set; }
    /// }
    /// ]]></code>
    /// </example>
    public PropertyInfo? InverseNavigationProperty { get; set; }

    /// <summary>
    /// The containing resource type in which this relationship is declared. Identical to <see cref="ResourceFieldAttribute.Type" />.
    /// </summary>
    public ResourceType LeftType => Type;

    /// <summary>
    /// The resource type this relationship points to. In the case of a <see cref="HasManyAttribute" /> relationship, this value will be the collection
    /// element type.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// public ISet<Tag> Tags { get; set; } // RightType: Tag
    /// ]]></code>
    /// </example>
    public ResourceType RightType
    {
        get => _rightType!;
        internal set
        {
            ArgumentGuard.NotNull(value, nameof(value));
            _rightType = value;
        }
    }

    /// <summary>
    /// Configures which links to write in the relationship-level links object for this relationship. Defaults to <see cref="LinkTypes.NotConfigured" />,
    /// which falls back to <see cref="ResourceLinksAttribute.RelationshipLinks" /> and then falls back to RelationshipLinks in global options.
    /// </summary>
    public LinkTypes Links { get; set; } = LinkTypes.NotConfigured;

    /// <summary>
    /// Whether or not this relationship can be included using the
    /// <c>
    /// ?include=publicName
    /// </c>
    /// query string parameter. This is <c>true</c> by default.
    /// </summary>
    public bool CanInclude { get; set; } = true;

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

        var other = (RelationshipAttribute)obj;

        return _rightType?.ClrType == other._rightType?.ClrType && Links == other.Links && CanInclude == other.CanInclude && base.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_rightType?.ClrType, Links, CanInclude, base.GetHashCode());
    }
}
