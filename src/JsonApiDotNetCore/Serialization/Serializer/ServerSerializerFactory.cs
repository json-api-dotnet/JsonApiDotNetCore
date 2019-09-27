using System;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Serialization.Serializer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Serialization.Serializer
{
    public class ServerSerializerFactory : IJsonApiSerializerFactory
    {
        private readonly ICurrentRequest _requestManager;
        private readonly IServiceProvider _provider;

        public ServerSerializerFactory(ICurrentRequest requestManager, IServiceProvider provider)
        {
            _requestManager = requestManager;
            _provider = provider;
        }
        public IJsonApiSerializer GetSerializer()
        {
            var serializerType = typeof(ServerSerializer<>).MakeGenericType(_requestManager.GetRequestResource().EntityType);
            return (IJsonApiSerializer)_provider.GetRequiredService(serializerType);
        }
    }
}
