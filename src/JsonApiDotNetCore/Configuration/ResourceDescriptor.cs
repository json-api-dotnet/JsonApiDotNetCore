using System;

namespace JsonApiDotNetCore.Configuration
{
    internal sealed class ResourceDescriptor
    {
        public Type ResourceType { get; }
        public Type IdType { get; }

        public ResourceDescriptor(Type resourceType, Type idType)
        {
            ResourceType = resourceType;
            IdType = idType;
        }
    }
}
