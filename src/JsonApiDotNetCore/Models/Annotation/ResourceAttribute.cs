using System;

namespace JsonApiDotNetCore.Models.Annotation
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ResourceAttribute : Attribute
    {
        public ResourceAttribute(string pluralizedResourceName)
        {
            ResourceName = pluralizedResourceName;
        }

        public string ResourceName { get; }
    }
}
