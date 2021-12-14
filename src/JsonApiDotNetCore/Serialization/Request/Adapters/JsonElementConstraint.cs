using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <summary>
/// Lists constraints for the presence or absence of a JSON element.
/// </summary>
[PublicAPI]
public enum JsonElementConstraint
{
    /// <summary>
    /// A value for the element is not allowed.
    /// </summary>
    Forbidden,

    /// <summary>
    /// A value for the element is required.
    /// </summary>
    Required
}
