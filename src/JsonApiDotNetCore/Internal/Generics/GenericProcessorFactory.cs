using JsonApiDotNetCore.Services;
using System;

namespace JsonApiDotNetCore.Internal.Generics
{
    /// <summary>
    /// Used to generate a generic operations processor when the types
    /// are not known until runtime. The typical use case would be for
    /// accessing relationship data or resolving operations processors.
    /// </summary>
    public interface IGenericProcessorFactory
    {
        /// <summary>
        /// Constructs the generic type and locates the service, then casts to TInterface
        /// </summary>
        /// <example>
        /// <code>
        ///     GetProcessor&lt;IGenericProcessor&gt;(typeof(GenericProcessor&lt;&gt;), typeof(TResource));
        /// </code>
        /// </example>
        TInterface GetProcessor<TInterface>(Type openGenericType, Type resourceType);

        /// <summary>
        /// Constructs the generic type and locates the service, then casts to TInterface
        /// </summary>
        /// <example>
        /// <code>
        ///     GetProcessor&lt;IGenericProcessor&gt;(typeof(GenericProcessor&lt;,&gt;), typeof(TResource), typeof(TId));
        /// </code>
        /// </example>
        TInterface GetProcessor<TInterface>(Type openGenericType, Type resourceType, Type keyType);
    }

    public class GenericProcessorFactory : IGenericProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GenericProcessorFactory(IScopedServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TInterface GetProcessor<TInterface>(Type openGenericType, Type resourceType)
            => GetProcessor<TInterface>(openGenericType, resourceType);

        public TInterface GetProcessor<TInterface>(Type openGenericType, Type resourceType, Type keyType)
            => GetProcessor<TInterface>(openGenericType, resourceType, keyType);

        private TInterface GetProcessor<TInterface>(Type openGenericType, params Type[] types)
        {
            var concreteType = openGenericType.MakeGenericType(types);

            return (TInterface)_serviceProvider.GetService(concreteType);
        }
    }
}
