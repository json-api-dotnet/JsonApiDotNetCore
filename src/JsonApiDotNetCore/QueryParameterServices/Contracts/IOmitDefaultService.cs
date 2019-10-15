namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?omitDefault=true
    /// </summary>
    public interface IOmitDefaultService : IQueryParameterService
    {
        /// <summary>
        /// Gets the parsed config
        /// </summary>
        bool Config { get; }
    }
}