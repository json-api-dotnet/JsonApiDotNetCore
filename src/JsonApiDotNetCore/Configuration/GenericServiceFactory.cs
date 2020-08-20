using System;

namespace JsonApiDotNetCore.Configuration
{
    public sealed class GenericServiceFactory : IGenericServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GenericServiceFactory(IScopedServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public TInterface Get<TInterface>(Type openGenericType, Type resourceType)
        {
            if (openGenericType == null) throw new ArgumentNullException(nameof(openGenericType));
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            return GetInternal<TInterface>(openGenericType, resourceType);
        }

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
