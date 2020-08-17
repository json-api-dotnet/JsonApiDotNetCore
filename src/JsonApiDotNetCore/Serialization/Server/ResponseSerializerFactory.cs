using System;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.RequestServices.Contracts;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <summary>
    /// A factory class to abstract away the initialization of the serializer from the
    /// .net core formatter pipeline.
    /// </summary>
    public class ResponseSerializerFactory : IJsonApiSerializerFactory
    {
        private readonly IServiceProvider _provider;
        private readonly ICurrentRequest _currentRequest;

        public ResponseSerializerFactory(ICurrentRequest currentRequest, IScopedServiceProvider provider)
        {
            _currentRequest = currentRequest;
            _provider = provider;
        }

        /// <summary>
        /// Initializes the server serializer using the <see cref="ResourceContext"/>
        /// associated with the current request.
        /// </summary>
        public IJsonApiSerializer GetSerializer()
        {
            var targetType = GetDocumentType();

            var serializerType = typeof(ResponseSerializer<>).MakeGenericType(targetType);
            var serializer = (IResponseSerializer)_provider.GetService(serializerType);
            if (_currentRequest.Kind == EndpointKind.Relationship && _currentRequest.Relationship != null)
                serializer.RequestRelationship = _currentRequest.Relationship;

            return (IJsonApiSerializer)serializer;
        }

        private Type GetDocumentType()
        {
            var resourceContext = _currentRequest.SecondaryResource ?? _currentRequest.PrimaryResource;
            return resourceContext.ResourceType;
        }
    }
}
