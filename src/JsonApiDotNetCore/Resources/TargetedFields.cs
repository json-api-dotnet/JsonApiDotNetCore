using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc cref="ITargetedFields" />
[PublicAPI]
public sealed class TargetedFields : ITargetedFields
{
    IReadOnlySet<AttrAttribute> ITargetedFields.Attributes => Attributes.AsReadOnly();
    IReadOnlySet<RelationshipAttribute> ITargetedFields.Relationships => Relationships.AsReadOnly();

    public HashSet<AttrAttribute> Attributes { get; } = [];
    public HashSet<RelationshipAttribute> Relationships { get; } = [];

    /// <inheritdoc />
    public void CopyFrom(ITargetedFields other)
    {
        ArgumentGuard.NotNull(other);

        Clear();

        Attributes.UnionWith(other.Attributes);
        Relationships.UnionWith(other.Relationships);
    }

    public void Clear()
    {
        Attributes.Clear();
        Relationships.Clear();
    }
}
