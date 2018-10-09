using System;

namespace JsonApiDotNetCore.Graph
{
    internal struct ResourceDescriptor
    {
        public ResourceDescriptor(Type resourceType, Type idType)
        {
            ResourceType = resourceType;
            IdType = idType;
        }

        public Type ResourceType { get; set; }
        public Type IdType { get; set; }

        internal static ResourceDescriptor Empty => new ResourceDescriptor(null, null);
    }
}
