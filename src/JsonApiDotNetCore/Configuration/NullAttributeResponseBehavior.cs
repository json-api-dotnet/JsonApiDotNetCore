namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Allows null attributes to be omitted from the response payload
    /// </summary>
    public struct NullAttributeResponseBehavior
    {
        /// <param name="omitNullValuedAttributes">Do not serialize null attributes</param>
        /// <param name="allowClientOverride">
        /// Allow clients to override the serialization behavior through a query parameter.
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
}
