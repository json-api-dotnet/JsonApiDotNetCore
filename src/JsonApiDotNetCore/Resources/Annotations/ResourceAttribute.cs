using System;

namespace JsonApiDotNetCore.Resources.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ResourceAttribute : Attribute
    {
        public ResourceAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }

        public string ResourceName { get; }
    }
}
