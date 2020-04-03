using System;

namespace JsonApiDotNetCore.Models
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
