using System;

namespace JsonApiDotNetCore.Configuration
{
    internal class ResourceDescriptor
    {
        public Type ResourceType { get; }
        public Type IdType { get; }

        internal static readonly ResourceDescriptor Empty = new ResourceDescriptor(null, null);

        public ResourceDescriptor(Type resourceType, Type idType)
        {
            ResourceType = resourceType;
            IdType = idType;
        }
    }
}
