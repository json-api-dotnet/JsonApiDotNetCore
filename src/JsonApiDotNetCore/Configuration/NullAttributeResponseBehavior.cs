namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Allows null attributes to be ommitted from the response payload
    /// </summary>
    public struct NullAttributeResponseBehavior
    {
        /// <param name="omitNullValuedAttributes">Do not serialize null attributes</param>
        /// <param name="allowClientOverride">
        /// Allow clients to override the serialization behavior through a query parmeter.
        /// <example>
        /// ```
        /// GET /articles?omitNullValuedAttributes=true
        /// ```
        /// </example>
        /// </param>
        public NullAttributeResponseBehavior(bool omitNullValuedAttributes = false, bool allowClientOverride = false)
        {
            OmitNullValuedAttributes = omitNullValuedAttributes;
            AllowClientOverride = allowClientOverride;
        }

        /// <summary>
        /// Do not include null attributes in the response payload.
        /// </summary>
        public bool OmitNullValuedAttributes { get; }

        /// <summary>
        /// Allows clients to specify a `omitNullValuedAttributes` boolean query param to control
        /// serialization behavior.
        /// </summary>
        public bool AllowClientOverride { get; }
    }

    public struct DefaultAttributeResponseBehavior
    {

        public DefaultAttributeResponseBehavior(bool omitNullValuedAttributes = false, bool allowClientOverride = false)
        {
            OmitDefaultValuedAttributes = omitNullValuedAttributes;
            AllowClientOverride = allowClientOverride;
        }

        public bool OmitDefaultValuedAttributes { get; }
        public bool AllowClientOverride { get; }
    }
}
