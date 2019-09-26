namespace JsonApiDotNetCore.Builders
{
    /// <summary>
    /// Options used to configure how a model gets serialized into
    /// a json:api document.
    /// </summary>
    public struct DocumentBuilderOptions
    {
        /// <param name="omitNullValuedAttributes">
        /// Do not serialize attributes with null values.
        /// </param>
        public DocumentBuilderOptions(bool omitNullValuedAttributes = false)
        {
            this.OmitNullValuedAttributes = omitNullValuedAttributes;
        }

        /// <summary>
        /// Prevent attributes with null values from being included in the response.
        /// This type is mostly internal and if you want to enable this behavior, you
        /// should do so on the <see ref="JsonApiDotNetCore.Configuration.JsonApiOptions" />.
        /// </summary>
        /// <example>
        /// <code>
        /// options.NullAttributeResponseBehavior = new NullAttributeResponseBehavior(true);
        /// </code>
        /// </example>
        public bool OmitNullValuedAttributes { get; private set; }
    }
}
