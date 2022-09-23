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
    public bool IsEmpty => !this.Any();

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
            return this.All(selector => selector.Key is RelationshipAttribute);
        }
    }

    public bool ContainsField(ResourceFieldAttribute field)
    {
        ArgumentGuard.NotNull(field);

        return ContainsKey(field);
    }

    public void IncludeAttribute(AttrAttribute attribute)
    {
        ArgumentGuard.NotNull(attribute);

        this[attribute] = null;
    }

    public void IncludeAttributes(IEnumerable<AttrAttribute> attributes)
    {
        ArgumentGuard.NotNull(attributes);

        foreach (AttrAttribute attribute in attributes)
        {
            this[attribute] = null;
        }
    }

    public void IncludeRelationship(RelationshipAttribute relationship, QueryLayer? queryLayer)
    {
        ArgumentGuard.NotNull(relationship);

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
