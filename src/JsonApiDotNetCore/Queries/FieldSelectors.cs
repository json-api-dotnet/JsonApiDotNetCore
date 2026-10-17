using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// A data structure that contains which fields (attributes and relationships) to retrieve, or empty to retrieve all. In the case of a relationship, it
/// contains the nested query constraints.
/// </summary>
[PublicAPI]
public sealed class FieldSelectors : Dictionary<ResourceFieldAttribute, QueryLayer?>
{
    private Dictionary<AttrAttribute, FieldSelectors>? _childSelectors;

    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Indicates whether any child selectors have been configured for compound attributes.
    /// </summary>
    public bool HasChildSelectors => _childSelectors is { Count: > 0 };

    /// <summary>
    /// The configured child selectors keyed by their parent compound attribute.
    /// </summary>
    public IReadOnlyDictionary<AttrAttribute, FieldSelectors> ChildSelectors =>
        _childSelectors ?? (IReadOnlyDictionary<AttrAttribute, FieldSelectors>)ImmutableDictionary<AttrAttribute, FieldSelectors>.Empty;

    /// <summary>
    /// Gets child selectors for the specified compound attribute, or <c>null</c> if not present.
    /// </summary>
    public FieldSelectors? GetChildSelectors(AttrAttribute compoundAttribute)
    {
        ArgumentNullException.ThrowIfNull(compoundAttribute);
        return _childSelectors?.GetValueOrDefault(compoundAttribute);
    }

    /// <summary>
    /// Gets or creates child selectors for the specified compound attribute.
    /// </summary>
    public FieldSelectors GetOrCreateChildSelectors(AttrAttribute compoundAttribute)
    {
        ArgumentNullException.ThrowIfNull(compoundAttribute);
        _childSelectors ??= new Dictionary<AttrAttribute, FieldSelectors>();

        if (!_childSelectors.TryGetValue(compoundAttribute, out FieldSelectors? childSelectors))
        {
            childSelectors = new FieldSelectors();
            _childSelectors[compoundAttribute] = childSelectors;
        }

        return childSelectors;
    }

    public bool ContainsReadOnlyAttribute
    {
        get
        {
            return this.Any(selector => selector.Key is AttrAttribute attribute && attribute.Property.SetMethod == null);
        }
    }

    public bool ContainsOnlyRelationships
    {
        get
        {
            return Count > 0 && this.All(selector => selector.Key is RelationshipAttribute);
        }
    }

    public bool ContainsField(ResourceFieldAttribute field)
    {
        ArgumentNullException.ThrowIfNull(field);

        return ContainsKey(field);
    }

    public void IncludeAttribute(AttrAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        this[attribute] = null;
    }

    /// <summary>
    /// Includes the compound attribute along with its nested child field selectors.
    /// </summary>
    public void IncludeCompoundAttribute(AttrAttribute compoundAttribute, FieldSelectors childSelectors)
    {
        ArgumentNullException.ThrowIfNull(compoundAttribute);
        ArgumentNullException.ThrowIfNull(childSelectors);

        this[compoundAttribute] = null;
        _childSelectors ??= new Dictionary<AttrAttribute, FieldSelectors>();
        _childSelectors[compoundAttribute] = childSelectors;
    }

    public void IncludeAttributes(IEnumerable<AttrAttribute> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        foreach (AttrAttribute attribute in attributes)
        {
            this[attribute] = null;
        }
    }

    public void IncludeRelationship(RelationshipAttribute relationship, QueryLayer queryLayer)
    {
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(queryLayer);

        this[relationship] = queryLayer;
    }

    public void RemoveAttributes()
    {
        while (this.Any(pair => pair.Key is AttrAttribute))
        {
            ResourceFieldAttribute field = this.First(pair => pair.Key is AttrAttribute).Key;
            Remove(field);
        }
    }
}
