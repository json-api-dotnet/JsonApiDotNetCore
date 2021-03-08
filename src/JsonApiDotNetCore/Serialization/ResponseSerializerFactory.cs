using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// A factory class to abstract away the initialization of the serializer from the ASP.NET Core formatter pipeline.
    /// </summary>
    [PublicAPI]
    public class ResponseSerializerFactory : IJsonApiSerializerFactory
    {
        private readonly IServiceProvider _provider;
        private readonly IJsonApiRequest _request;

        public ResponseSerializerFactory(IJsonApiRequest request, IRequestScopedServiceProvider provider)
        {
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(provider, nameof(provider));

            _request = request;
            _provider = provider;
        }

        /// <summary>
        /// Initializes the server serializer using the <see cref="ResourceContext" /> associated with the current request.
        /// </summary>
        public IJsonApiSerializer GetSerializer()
        {
            if (_request.Kind == EndpointKind.AtomicOperations)
            {
                return (IJsonApiSerializer)_provider.GetRequiredService(typeof(AtomicOperationsResponseSerializer));
            }

            Type targetType = GetDocumentType();

            Type serializerType = typeof(ResponseSerializer<>).MakeGenericType(targetType);
            object serializer = _provider.GetRequiredService(serializerType);

            return (IJsonApiSerializer)serializer;
        }

        private Type GetDocumentType()
        {
            ResourceContext resourceContext = _request.SecondaryResource ?? _request.PrimaryResource;
            return resourceContext.ResourceType;
        }
    }
}
