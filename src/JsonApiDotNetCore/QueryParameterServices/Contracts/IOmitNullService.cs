namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?omitNull=true
    /// </summary>
    public interface IOmitNullService : IQueryParameterService
    {
        /// <summary>
        /// Contains the effective value of default configuration and query string override, after parsing has occured.
        /// </summary>
        bool OmitAttributeIfValueIsNull { get; }
    }
}
