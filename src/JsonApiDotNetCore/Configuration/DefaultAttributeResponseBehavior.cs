namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Determines how attributes that contain a default value are serialized in the response payload.
    /// </summary>
    public struct DefaultAttributeResponseBehavior
    {
        /// <param name="omitAttributeIfValueIsDefault">Determines whether to serialize attributes that contain their types' default value.</param>
        /// <param name="allowQueryStringOverride">Determines whether serialization behavior can be controlled by a query string parameter.</param>
        public DefaultAttributeResponseBehavior(bool omitAttributeIfValueIsDefault = false, bool allowQueryStringOverride = false)
        {
            OmitAttributeIfValueIsDefault = omitAttributeIfValueIsDefault;
            AllowQueryStringOverride = allowQueryStringOverride;
        }

        /// <summary>
        /// Determines whether to serialize attributes that contain their types' default value.
        /// </summary>
        public bool OmitAttributeIfValueIsDefault { get; }

        /// <summary>
        /// Determines whether serialization behavior can be controlled by a query string parameter.
        /// </summary>
        public bool AllowQueryStringOverride { get; }
    }
}
