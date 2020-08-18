using System;

namespace JsonApiDotNetCore.Configuration
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
}
