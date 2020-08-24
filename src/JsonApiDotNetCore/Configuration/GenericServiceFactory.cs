using System;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    public sealed class GenericServiceFactory : IGenericServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GenericServiceFactory(IRequestScopedServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public TInterface Get<TInterface>(Type openGenericType, Type resourceType)
        {
            if (openGenericType == null) throw new ArgumentNullException(nameof(openGenericType));
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            return GetInternal<TInterface>(openGenericType, resourceType);
        }

        /// <inheritdoc />
        public TInterface Get<TInterface>(Type openGenericType, Type resourceType, Type keyType)
        {
            if (openGenericType == null) throw new ArgumentNullException(nameof(openGenericType));
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));
            if (keyType == null) throw new ArgumentNullException(nameof(keyType));

            return GetInternal<TInterface>(openGenericType, resourceType, keyType);
        }

        private TInterface GetInternal<TInterface>(Type openGenericType, params Type[] types)
        {
            var concreteType = openGenericType.MakeGenericType(types);

            return (TInterface)_serviceProvider.GetService(concreteType);
        }
    }
}
