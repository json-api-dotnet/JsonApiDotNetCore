using System;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    internal sealed class ResourceTypeInfo
    {
        public Type ResourceObjectType { get; }
        public Type ResourceObjectOpenType { get; }
        public ResourceType ResourceType { get; }

        private ResourceTypeInfo(Type resourceObjectType, Type resourceObjectOpenType, ResourceType resourceType)
        {
            ResourceObjectType = resourceObjectType;
            ResourceObjectOpenType = resourceObjectOpenType;
            ResourceType = resourceType;
        }

        public static ResourceTypeInfo Create(Type resourceObjectType, IResourceGraph resourceGraph)
        {
            ArgumentGuard.NotNull(resourceObjectType, nameof(resourceObjectType));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            Type resourceObjectOpenType = resourceObjectType.GetGenericTypeDefinition();
            Type resourceClrType = resourceObjectType.GenericTypeArguments[0];
            ResourceType resourceType = resourceGraph.GetResourceType(resourceClrType);

            return new ResourceTypeInfo(resourceObjectType, resourceObjectOpenType, resourceType);
        }
    }
}
