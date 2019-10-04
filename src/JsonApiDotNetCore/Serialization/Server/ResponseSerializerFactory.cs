using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Serialization.Request.Contracts;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <summary>
    /// A factory class to abstract away the initialization of the serializer from the
    /// .net core formatter pipeline.
    /// </summary>
    public class ResponseSerializerFactory : IJsonApiSerializerFactory
    {
        private readonly ICurrentRequest _currentRequest;
        private readonly IScopedServiceProvider _provider;

        public ResponseSerializerFactory(ICurrentRequest currentRequest, IScopedServiceProvider provider)
        {
            _currentRequest = currentRequest;
            _provider = provider;
        }

        /// <summary>
        /// Initializes the server serializer using the <see cref="ContextEntity"/>
        /// associated with the current request.
        /// </summary>
        public IJsonApiSerializer GetSerializer()
        {   
            var serializerType = typeof(ResponseSerializer<>).MakeGenericType(_currentRequest.GetRequestResource().EntityType);
            return (IJsonApiSerializer)_provider.GetRequiredService(serializerType);
        }
    }
}
