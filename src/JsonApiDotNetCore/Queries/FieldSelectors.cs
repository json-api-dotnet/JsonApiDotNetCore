using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// A data structure that contains which fields (attributes and relationships) to retrieve, or empty to retrieve all. In the case of a relationship, it
/// contains the nested query constraints.
/// </summary>
[PublicAPI]
public sealed class FieldSelectors : Dictionary<ResourceFieldChainExpression, QueryLayer?>
{
    public bool IsEmpty => Count == 0;

    public bool ContainsReadOnlyAttribute
    {
        get
        {
            return this.Any(selector => selector.Key.Fields[0] is AttrAttribute attribute && attribute.Property.SetMethod == null);
        }
    }

    public bool ContainsOnlyRelationships
    {
        get
        {
            return Count > 0 && this.All(selector => selector.Key.Fields[0] is RelationshipAttribute);
        }
    }

    public bool ContainsField(ResourceFieldAttribute field)
    {
        ArgumentNullException.ThrowIfNull(field);

        return ContainsKey(new ResourceFieldChainExpression(field));
    }

    public void IncludeAttribute(ResourceFieldChainExpression attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        this[attribute] = null;
    }

    public void IncludeAttributes(IEnumerable<ResourceFieldChainExpression> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        foreach (var attribute in attributes)
        {
            this[attribute] = null;
        }
    }

    public void IncludeRelationship(RelationshipAttribute relationship, QueryLayer queryLayer)
    {
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(queryLayer);

        this[new ResourceFieldChainExpression(relationship)] = queryLayer;
    }

    public void RemoveAttributes()
    {
        while (this.Any(pair => pair.Key.Fields[0] is AttrAttribute))
        {
            var field = this.First(pair => pair.Key.Fields[0] is AttrAttribute).Key;
            Remove(field);
        }
    }
}
