using System;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    internal sealed class ResourceTypeInfo
    {
        private readonly ResourceContext _resourceContext;

        public Type ResourceObjectType { get; }
        public Type ResourceObjectOpenType { get; }
        public Type ResourceType { get; }

        private ResourceTypeInfo(Type resourceObjectType, Type resourceObjectOpenType, Type resourceType, ResourceContext resourceContext)
        {
            _resourceContext = resourceContext;

            ResourceObjectType = resourceObjectType;
            ResourceObjectOpenType = resourceObjectOpenType;
            ResourceType = resourceType;
        }

        public static ResourceTypeInfo Create(Type resourceObjectType, IResourceGraph resourceGraph)
        {
            ArgumentGuard.NotNull(resourceObjectType, nameof(resourceObjectType));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            Type resourceObjectOpenType = resourceObjectType.GetGenericTypeDefinition();
            Type resourceType = resourceObjectType.GenericTypeArguments[0];
            ResourceContext resourceContext = resourceGraph.GetResourceContext(resourceType);

            return new ResourceTypeInfo(resourceObjectType, resourceObjectOpenType, resourceType, resourceContext);
        }

        public TResourceFieldAttribute TryGetResourceFieldByName<TResourceFieldAttribute>(string publicName)
            where TResourceFieldAttribute : ResourceFieldAttribute
        {
            ArgumentGuard.NotNullNorEmpty(publicName, nameof(publicName));

            return (TResourceFieldAttribute)_resourceContext.Fields.FirstOrDefault(field => field is TResourceFieldAttribute && field.PublicName == publicName);
        }
    }
}
