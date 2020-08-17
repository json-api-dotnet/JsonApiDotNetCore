using JsonApiDotNetCore.Services;
using System;

namespace JsonApiDotNetCore.Internal.Generics
{
    /// <summary>
    /// Used to generate a generic operations processor when the types
    /// are not known until runtime. The typical use case would be for
    /// accessing relationship data or resolving operations processors.
    /// </summary>
    public interface IGenericServiceFactory
    {
        /// <summary>
        /// Constructs the generic type and locates the service, then casts to TInterface
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        ///     Get<IGenericProcessor>(typeof(GenericProcessor<>), typeof(TResource));
        /// ]]></code>
        /// </example>
        TInterface Get<TInterface>(Type openGenericType, Type resourceType);

        /// <summary>
        /// Constructs the generic type and locates the service, then casts to TInterface
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        ///     Get<IGenericProcessor>(typeof(GenericProcessor<>), typeof(TResource), typeof(TId));
        /// ]]></code>
        /// </example>
        TInterface Get<TInterface>(Type openGenericType, Type resourceType, Type keyType);
    }

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
