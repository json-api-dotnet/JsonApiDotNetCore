using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc />
[PublicAPI]
public sealed class TargetedFields : ITargetedFields
{
    IReadOnlySet<AttrAttribute> ITargetedFields.Attributes => Attributes;
    IReadOnlySet<RelationshipAttribute> ITargetedFields.Relationships => Relationships;

    public HashSet<AttrAttribute> Attributes { get; } = new();
    public HashSet<RelationshipAttribute> Relationships { get; } = new();

    /// <inheritdoc />
    public void CopyFrom(ITargetedFields other)
    {
        Clear();

        Attributes.AddRange(other.Attributes);
        Relationships.AddRange(other.Relationships);
    }

    public void Clear()
    {
        Attributes.Clear();
        Relationships.Clear();
    }
}
