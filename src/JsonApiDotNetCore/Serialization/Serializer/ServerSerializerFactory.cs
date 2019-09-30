using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Serialization.Serializer.Contracts;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Serialization.Serializer
{
    /// <summary>
    /// A factory class to abstract away the initialization of the serializer from the
    /// .net core formatter pipeline.
    /// </summary>
    public class ServerSerializerFactory : IJsonApiSerializerFactory
    {
        private readonly ICurrentRequest _requestManager;
        private readonly IScopedServiceProvider _provider;

        public ServerSerializerFactory(ICurrentRequest requestManager, IScopedServiceProvider provider)
        {
            _requestManager = requestManager;
            _provider = provider;
        }

        /// <summary>
        /// Initializes the server serializer using the <see cref="ContextEntity"/>
        /// associated with the current request.
        /// </summary>
        public IJsonApiSerializer GetSerializer()
        {   
            var serializerType = typeof(ServerSerializer<>).MakeGenericType(_requestManager.GetRequestResource().EntityType);
            return (IJsonApiSerializer)_provider.GetRequiredService(serializerType);
        }
    }
}
