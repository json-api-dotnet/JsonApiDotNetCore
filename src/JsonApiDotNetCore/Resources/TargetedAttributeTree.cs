using System.Diagnostics;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc cref="ITargetedAttributeTree" />
[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class TargetedAttributeTree : ITargetedAttributeTree
{
    /// <inheritdoc />
    AttrAttribute ITargetedAttributeTree.Attribute => Attribute;

    /// <inheritdoc />
    IReadOnlySet<ITargetedAttributeTree> ITargetedAttributeTree.Children => Children.Cast<ITargetedAttributeTree>().ToHashSet().AsReadOnly();

    /// <inheritdoc cref="ITargetedAttributeTree.Attribute" />
    public AttrAttribute Attribute { get; set; }

    /// <inheritdoc cref="ITargetedAttributeTree.Children" />
    public HashSet<TargetedAttributeTree> Children { get; }

    public TargetedAttributeTree(AttrAttribute attribute, HashSet<TargetedAttributeTree> children)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        ArgumentNullException.ThrowIfNull(children);

        Attribute = attribute;
        Children = children;
    }

    /// <inheritdoc />
    public void Apply<T>(T source, T target)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (Attribute.Kind != AttrKind.Compound)
        {
            object? sourceValue = Attribute.GetValue(source);
            Attribute.SetValue(target, sourceValue);
        }
        else
        {
            object? subSourceInstance = Attribute.GetValue(source);
            object? subTargetInstance = Attribute.GetValue(target);

            if (subSourceInstance == null)
            {
                Attribute.SetValue(target, null);
            }
            else
            {
                if (Children.Count > 0)
                {
                    if (subTargetInstance == null)
                    {
                        subTargetInstance = Activator.CreateInstance(Attribute.Property.PropertyType);
                        Attribute.SetValue(target, subTargetInstance);
                    }

                    if (subTargetInstance != null)
                    {
                        foreach (TargetedAttributeTree child in Children)
                        {
                            child.Apply(subSourceInstance, subTargetInstance);
                        }
                    }
                }
            }
        }
    }

    public override string ToString()
    {
        if (Children.Count == 0)
        {
            return $"{Attribute}";
        }

        // Example: contact { displayName, livingAddress { street, city }, phoneNumber }
        return $"{Attribute} {{ {string.Join(", ", Children.Select(child => child.ToString()))} }}";
    }
}
