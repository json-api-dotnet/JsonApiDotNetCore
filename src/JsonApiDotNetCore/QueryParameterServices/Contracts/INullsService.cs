namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?nulls=false
    /// </summary>
    public interface INullsService : IQueryParameterService
    {
        /// <summary>
        /// Contains the effective value of default configuration and query string override, after parsing has occured.
        /// </summary>
        bool OmitAttributeIfValueIsNull { get; }
    }
}
