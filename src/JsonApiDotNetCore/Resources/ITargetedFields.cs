using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <summary>
/// Container to register which resource fields (attributes and relationships) are targeted by a request.
/// </summary>
public interface ITargetedFields
{
    /// <summary>
    /// The set of attributes that are targeted by a request.
    /// </summary>
    IReadOnlySet<AttrAttribute> Attributes { get; }

    /// <summary>
    /// The set of relationships that are targeted by a request.
    /// </summary>
    IReadOnlySet<RelationshipAttribute> Relationships { get; }

    /// <summary>
    /// Performs a shallow copy.
    /// </summary>
    void CopyFrom(ITargetedFields other);
}
