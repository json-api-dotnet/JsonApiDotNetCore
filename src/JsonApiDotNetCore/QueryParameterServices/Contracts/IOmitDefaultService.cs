namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?omitDefault=true
    /// </summary>
    public interface IOmitDefaultService : IQueryParameterService
    {
        /// <summary>
        /// Contains the effective value of default configuration and query string override, after parsing has occured.
        /// </summary>
        bool OmitAttributeIfValueIsDefault { get; }
    }
}
