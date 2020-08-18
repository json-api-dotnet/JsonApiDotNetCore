using System;

namespace JsonApiDotNetCore.Configuration
{
    public sealed class GenericServiceFactory : IGenericServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GenericServiceFactory(IScopedServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TInterface Get<TInterface>(Type openGenericType, Type resourceType)
            => GetInternal<TInterface>(openGenericType, resourceType);

        public TInterface Get<TInterface>(Type openGenericType, Type resourceType, Type keyType)
            => GetInternal<TInterface>(openGenericType, resourceType, keyType);

        private TInterface GetInternal<TInterface>(Type openGenericType, params Type[] types)
        {
            var concreteType = openGenericType.MakeGenericType(types);

            return (TInterface)_serviceProvider.GetService(concreteType);
        }
    }
}
