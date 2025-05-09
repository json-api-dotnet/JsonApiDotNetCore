using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc cref="ITargetedFields" />
[PublicAPI]
public sealed class TargetedFields : ITargetedFields
{
    /// <inheritdoc />
    IReadOnlySet<ITargetedAttributeTree> ITargetedFields.Attributes => Attributes.Cast<ITargetedAttributeTree>().ToHashSet().AsReadOnly();

    /// <inheritdoc />
    IReadOnlySet<RelationshipAttribute> ITargetedFields.Relationships => Relationships.AsReadOnly();

    /// <inheritdoc cref="ITargetedFields.Attributes" />
    public HashSet<TargetedAttributeTree> Attributes { get; } = [];

    /// <inheritdoc cref="ITargetedFields.Relationships" />
    public HashSet<RelationshipAttribute> Relationships { get; } = [];

    /// <inheritdoc />
    public void CopyFrom(ITargetedFields other)
    {
        ArgumentNullException.ThrowIfNull(other);

        Clear();

        CopyTargetedAttributesFrom(other.Attributes);
        Relationships.UnionWith(other.Relationships);
    }

    private void CopyTargetedAttributesFrom(IReadOnlySet<ITargetedAttributeTree> otherTargets)
    {
        foreach (ITargetedAttributeTree otherTarget in otherTargets)
        {
            TargetedAttributeTree thisTarget = ToMutable(otherTarget);
            Attributes.Add(thisTarget);
        }
    }

    private static TargetedAttributeTree ToMutable(ITargetedAttributeTree target)
    {
        return new TargetedAttributeTree(target.Attribute, target.Children.Select(ToMutable).ToHashSet());
    }

    public void Clear()
    {
        Attributes.Clear();
        Relationships.Clear();
    }
}
