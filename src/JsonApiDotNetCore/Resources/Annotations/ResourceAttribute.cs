using System;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// When put on a resource class, overrides the convention-based resource name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ResourceAttribute : Attribute
    {
        /// <summary>
        /// The publicly exposed name of this resource type.
        /// When not explicitly assigned, the configured casing convention is applied on the pluralized resource class name.
        /// </summary>
        public string PublicName { get; }

        public ResourceAttribute(string publicName)
        {
            PublicName = publicName ?? throw new ArgumentNullException(nameof(publicName));
        }
    }
}
