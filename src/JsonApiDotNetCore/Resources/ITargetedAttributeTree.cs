using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <summary>
/// The data structure for an attribute targeted by a request.
/// </summary>
public interface ITargetedAttributeTree
{
    /// <summary>
    /// Gets the attribute being targeted.
    /// </summary>
    AttrAttribute Attribute { get; }

    /// <summary>
    /// Gets the set of child attributes being targeted.
    /// </summary>
    IReadOnlySet<ITargetedAttributeTree> Children { get; }

    /// <summary>
    /// Recursively applies targeted attributes by copying property values from source to target object.
    /// </summary>
    /// <param name="source">
    /// The source object to copy from.
    /// </param>
    /// <param name="target">
    /// The target object to copy to.
    /// </param>
    void Apply<T>(T source, T target)
        where T : notnull;
}
