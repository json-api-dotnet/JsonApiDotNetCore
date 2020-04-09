using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Options used to configure how fields of a model get serialized into
    /// a json:api <see cref="Document"/>.
    /// </summary>
    public sealed class ResourceObjectBuilderSettings 
    {
        /// <param name="omitAttributeIfValueIsNull">Omit null values from attributes</param>
        /// <param name="omitAttributeIfValueIsDefault">Omit default values from attributes</param>
        public ResourceObjectBuilderSettings(bool omitAttributeIfValueIsNull = false, bool omitAttributeIfValueIsDefault = false)
        {
            OmitAttributeIfValueIsNull = omitAttributeIfValueIsNull;
            OmitAttributeIfValueIsDefault = omitAttributeIfValueIsDefault;
        }

        /// <summary>
        /// Prevent attributes with null values from being included in the response.
        /// This property is internal and if you want to enable this behavior, you
        /// should do so on the <see cref="IJsonApiOptions.SerializerSettings" />.
        /// </summary>
        /// <example>
        /// <code>
        /// options.NullAttributeResponseBehavior = new NullAttributeResponseBehavior(true);
        /// </code>
        /// </example>
        public bool OmitAttributeIfValueIsNull { get; }

        /// <summary>
        /// Prevent attributes with default values from being included in the response.
        /// This property is internal and if you want to enable this behavior, you
        /// should do so on the <see cref="IJsonApiOptions.SerializerSettings" />.
        /// </summary>
        /// <example>
        /// <code>
        /// options.DefaultAttributeResponseBehavior = new DefaultAttributeResponseBehavior(true);
        /// </code>
        /// </example>
        public bool OmitAttributeIfValueIsDefault { get; }
    }
}
