namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Base interface that all query parameter services should inherit.
    /// </summary>
    internal interface IQueryParameterService
    {
        /// <summary>
        /// Parses the value of the query parameter. Invoked in the middleware.
        /// </summary>
        /// <param name="value">the value of the query parameter as parsed from the url</param>
        void Parse(string value);
        /// <summary>
        /// The name of the query parameter as matched in the URL.
        /// </summary>
        string Name { get; }
    }
}
