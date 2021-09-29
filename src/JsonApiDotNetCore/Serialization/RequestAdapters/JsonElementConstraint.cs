using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <summary>
    /// Lists constraints for the presence or absence of a JSON element.
    /// </summary>
    [PublicAPI]
    public enum JsonElementConstraint
    {
        /// <summary>
        /// A value for the field is not allowed.
        /// </summary>
        Forbidden,

        /// <summary>
        /// A value for the field is required.
        /// </summary>
        Required
    }
}
