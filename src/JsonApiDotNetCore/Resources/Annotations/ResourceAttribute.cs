using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// When put on a resource class, overrides the convention-based resource name.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ResourceAttribute : Attribute
    {
        /// <summary>
        /// The publicly exposed name of this resource type. When not explicitly assigned, the configured naming convention is applied on the pluralized resource
        /// class name.
        /// </summary>
        public string PublicName { get; }

        public ResourceAttribute(string publicName)
        {
            ArgumentGuard.NotNullNorEmpty(publicName, nameof(publicName));

            PublicName = publicName;
        }
    }
}
