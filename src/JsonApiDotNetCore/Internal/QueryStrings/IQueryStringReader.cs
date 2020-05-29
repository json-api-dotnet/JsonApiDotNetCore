using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    /// <summary>
    /// Reads and processes the various query string parameters.
    /// </summary>
    public interface IQueryStringReader
    {
        /// <summary>
        /// Reads and processes the key/value pairs from the request query string.
        /// </summary>
        /// <param name="disableQueryAttribute">
        /// The <see cref="DisableQueryAttribute"/> if set on the controller that is targeted by the current request.
        /// </param>
        void ReadAll(DisableQueryAttribute disableQueryAttribute);
    }
}
