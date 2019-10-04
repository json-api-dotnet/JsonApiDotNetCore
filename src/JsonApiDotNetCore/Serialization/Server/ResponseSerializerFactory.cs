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
        private readonly IScopedServiceProvider _provider;

        public ResponseSerializerFactory(IScopedServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Initializes the server serializer using the <see cref="ContextEntity"/>
        /// associated with the current request.
        /// </summary>
        public IJsonApiSerializer GetSerializer(Type targetType)
        {   
            var serializerType = typeof(ResponseSerializer<>).MakeGenericType(ExtractResourceType(targetType));
            return (IJsonApiSerializer)_provider.GetRequiredService(serializerType);
        }

        private Type ExtractResourceType(Type type)
        {
            if (type.Inherits<IIdentifiable>())
                return type;

            return TypeHelper.GetTypeOfList(type);
        }
    }
}
