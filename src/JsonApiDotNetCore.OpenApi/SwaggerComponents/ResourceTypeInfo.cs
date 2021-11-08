using System;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    internal sealed class ResourceTypeInfo
    {
        private readonly ResourceType _resourceType;

        public Type ResourceObjectType { get; }
        public Type ResourceObjectOpenType { get; }
        public Type ResourceClrType { get; }

        private ResourceTypeInfo(Type resourceObjectType, Type resourceObjectOpenType, Type resourceClrType, ResourceType resourceType)
        {
            _resourceType = resourceType;

            ResourceObjectType = resourceObjectType;
            ResourceObjectOpenType = resourceObjectOpenType;
            ResourceClrType = resourceClrType;
        }

        public static ResourceTypeInfo Create(Type resourceObjectType, IResourceGraph resourceGraph)
        {
            ArgumentGuard.NotNull(resourceObjectType, nameof(resourceObjectType));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            Type resourceObjectOpenType = resourceObjectType.GetGenericTypeDefinition();
            Type resourceClrType = resourceObjectType.GenericTypeArguments[0];
            ResourceType resourceType = resourceGraph.GetResourceType(resourceClrType);

            return new ResourceTypeInfo(resourceObjectType, resourceObjectOpenType, resourceClrType, resourceType);
        }

        public TResourceFieldAttribute? FindResourceFieldByName<TResourceFieldAttribute>(string publicName)
            where TResourceFieldAttribute : ResourceFieldAttribute
        {
            ArgumentGuard.NotNullNorEmpty(publicName, nameof(publicName));

            return (TResourceFieldAttribute?)_resourceType.Fields.FirstOrDefault(field => field is TResourceFieldAttribute && field.PublicName == publicName);
        }
    }
}
