using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Represents the Service Locator design pattern. Used to obtain object instances for types are not known until runtime. This is only used by resource
    /// hooks and subject to be removed in a future version.
    /// </summary>
    [PublicAPI]
    public interface IGenericServiceFactory
    {
        /// <summary>
        /// Constructs the generic type and locates the service, then casts to <typeparamref name="TInterface" />.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// Get<IGenericProcessor>(typeof(GenericProcessor<>), typeof(TResource));
        /// ]]></code>
        /// </example>
        TInterface Get<TInterface>(Type openGenericType, Type resourceType);

        /// <summary>
        /// Constructs the generic type and locates the service, then casts to <typeparamref name="TInterface" />.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// Get<IGenericProcessor>(typeof(GenericProcessor<>), typeof(TResource), typeof(TId));
        /// ]]></code>
        /// </example>
        TInterface Get<TInterface>(Type openGenericType, Type resourceType, Type keyType);
    }
}
