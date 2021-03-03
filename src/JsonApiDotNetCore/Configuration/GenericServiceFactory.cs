using System;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    public sealed class GenericServiceFactory : IGenericServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GenericServiceFactory(IRequestScopedServiceProvider serviceProvider)
        {
            ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public TInterface Get<TInterface>(Type openGenericType, Type resourceType)
        {
            ArgumentGuard.NotNull(openGenericType, nameof(openGenericType));
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            return GetInternal<TInterface>(openGenericType, resourceType);
        }

        /// <inheritdoc />
        public TInterface Get<TInterface>(Type openGenericType, Type resourceType, Type keyType)
        {
            ArgumentGuard.NotNull(openGenericType, nameof(openGenericType));
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));
            ArgumentGuard.NotNull(keyType, nameof(keyType));

            return GetInternal<TInterface>(openGenericType, resourceType, keyType);
        }

        private TInterface GetInternal<TInterface>(Type openGenericType, params Type[] types)
        {
            Type concreteType = openGenericType.MakeGenericType(types);

            return (TInterface)_serviceProvider.GetService(concreteType);
        }
    }
}
