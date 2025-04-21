using System.Collections;
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
            Type? elementType = CollectionConverter.Instance.FindCollectionElementType(InverseNavigationProperty.PropertyType);
            return elementType != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override void SetValue(object resource, object? newValue)
    {
        ArgumentNullException.ThrowIfNull(newValue);
        AssertIsIdentifiableCollection(newValue);

        base.SetValue(resource, newValue);
    }

    private void AssertIsIdentifiableCollection(object newValue)
    {
        if (newValue is not IEnumerable enumerable)
        {
            throw new InvalidOperationException($"Resource of type '{newValue.GetType()}' must be a collection.");
        }

        foreach (object? element in enumerable)
        {
            if (element == null)
            {
                throw new InvalidOperationException("Resource collection must not contain null values.");
            }

            AssertIsIdentifiable(element);
        }
    }

    /// <summary>
    /// Adds a resource to this to-many relationship on the specified resource instance. Throws if the property is read-only or if the field does not belong
    /// to the specified resource instance.
    /// </summary>
    public void AddValue(object resource, IIdentifiable resourceToAdd)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceToAdd);

        object? rightValue = GetValue(resource);
        List<IIdentifiable> rightResources = CollectionConverter.Instance.ExtractResources(rightValue).ToList();

        if (!rightResources.Exists(nextResource => nextResource == resourceToAdd))
        {
            rightResources.Add(resourceToAdd);

            Type collectionType = rightValue?.GetType() ?? Property.PropertyType;
            IEnumerable typedCollection = CollectionConverter.Instance.CopyToTypedCollection(rightResources, collectionType);
            base.SetValue(resource, typedCollection);
        }
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

        var other = (HasManyAttribute)obj;

        return _capabilities == other._capabilities && base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_capabilities, base.GetHashCode());
    }
}
