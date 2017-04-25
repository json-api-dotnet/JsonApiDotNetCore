using System;

namespace JsonApiDotNetCore.Models
{
    public class ResourceAttribute : Attribute
    {
        public ResourceAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }

        public string ResourceName { get; set; }
    }
}
