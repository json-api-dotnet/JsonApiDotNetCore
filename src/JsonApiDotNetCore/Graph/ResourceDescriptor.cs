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

        public Type ResourceType { get; }
        public Type IdType { get; }

        internal static ResourceDescriptor Empty { get; } = new ResourceDescriptor(null, null);
    }
}
