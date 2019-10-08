using System;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
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
        private readonly IServiceProvider _provider;
        private readonly ICurrentRequest _currentRequest;

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
            var targetType = GetDocumentPrimaryType();
            if (targetType == null)
                return null;

            var serializerType = typeof(ResponseSerializer<>).MakeGenericType(targetType);
            var serializer = (IJsonApiDefaultSerializer)_provider.GetService(serializerType);
            if (_currentRequest.RequestRelationship != null && _currentRequest.IsRelationshipPath)
                serializer.SetRequestRelationship(_currentRequest.RequestRelationship);

            return serializer;
        }

        private Type GetDocumentPrimaryType()
        {
            if (_currentRequest.RequestRelationship != null && !_currentRequest.IsRelationshipPath)
                return _currentRequest.RequestRelationship.DependentType;

            return _currentRequest.GetRequestResource()?.EntityType;
        }
    }
}
