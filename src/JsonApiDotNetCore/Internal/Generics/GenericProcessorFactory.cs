using Microsoft.AspNetCore.Http;
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
        ///     GetProcessor&lt;IGenericProcessor&gt;(typeof(GenericProcessor&lt;&gt;), typeof(TResource));
        /// </example>
        TInterface GetProcessor<TInterface>(Type openGenericType, Type resourceType);

        /// <summary>
        /// Constructs the generic type and locates the service, then casts to TInterface
        /// </summary>
        /// <example>
        ///     GetProcessor&lt;IGenericProcessor&gt;(typeof(GenericProcessor&lt;,&gt;), typeof(TResource), typeof(TId));
        /// </example>
        TInterface GetProcessor<TInterface>(Type openGenericType, Type resourceType, Type keyType);
    }

    public class GenericProcessorFactory : IGenericProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GenericProcessorFactory(IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = httpContextAccessor.HttpContext.RequestServices;
        }

        public TInterface GetProcessor<TInterface>(Type openGenericType, Type resourceType)
            => _getProcessor<TInterface>(openGenericType, resourceType);

        public TInterface GetProcessor<TInterface>(Type openGenericType, Type resourceType, Type keyType)
            => _getProcessor<TInterface>(openGenericType, resourceType, keyType);

        private TInterface _getProcessor<TInterface>(Type openGenericType, params Type[] types)
        {
            var concreteType = openGenericType.MakeGenericType(types);

            return (TInterface)_serviceProvider.GetService(concreteType);
        }
    }
}
