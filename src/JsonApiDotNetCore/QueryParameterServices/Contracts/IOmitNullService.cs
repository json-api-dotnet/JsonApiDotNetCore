namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?omitNull=true
    /// </summary>
    public interface IOmitNullService : IQueryParameterService
    {
        /// <summary>
        /// Gets the parsed config
        /// </summary>
        bool Config { get; }
    }
}