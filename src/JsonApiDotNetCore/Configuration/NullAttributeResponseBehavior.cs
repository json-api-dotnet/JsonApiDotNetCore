namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Determines how attributes that contain null are serialized in the response payload.
    /// </summary>
    public struct NullAttributeResponseBehavior
    {
        /// <param name="omitAttributeIfValueIsNull">Determines whether to serialize attributes that contain null.</param>
        /// <param name="allowQueryStringOverride">Determines whether serialization behavior can be controlled by a query string parameter.</param>
        public NullAttributeResponseBehavior(bool omitAttributeIfValueIsNull = false, bool allowQueryStringOverride = false)
        {
            OmitAttributeIfValueIsNull = omitAttributeIfValueIsNull;
            AllowQueryStringOverride = allowQueryStringOverride;
        }

        /// <summary>
        /// Determines whether to serialize attributes that contain null.
        /// </summary>
        public bool OmitAttributeIfValueIsNull { get; }

        /// <summary>
        /// Determines whether serialization behavior can be controlled by a query string parameter.
        /// </summary>
        public bool AllowQueryStringOverride { get; }
    }
}
