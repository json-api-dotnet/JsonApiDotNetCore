namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Allows default valued attributes to be ommitted from the response payload
    /// </summary>
    public struct DefaultAttributeResponseBehavior
    {

        /// <param name="omitNullValuedAttributes">Do not serialize default value attributes</param>
        /// <param name="allowClientOverride">
        /// Allow clients to override the serialization behavior through a query parmeter.
        /// <example>
        /// ```
        /// GET /articles?omitDefaultValuedAttributes=true
        /// ```
        /// </example>
        /// </param>
        public DefaultAttributeResponseBehavior(bool omitNullValuedAttributes = false, bool allowClientOverride = false)
        {
            OmitDefaultValuedAttributes = omitNullValuedAttributes;
            AllowClientOverride = allowClientOverride;
        }

        /// <summary>
        /// Do (not) include default valued attributes in the response payload.
        /// </summary>
        public bool OmitDefaultValuedAttributes { get; }

        /// <summary>
        /// Allows clients to specify a `omitDefaultValuedAttributes` boolean query param to control
        /// serialization behavior.
        /// </summary>
        public bool AllowClientOverride { get; }
    }
}
