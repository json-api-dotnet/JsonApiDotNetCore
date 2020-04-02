using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Responsible for populating the various service implementations of <see cref="IQueryParameterService"/>.
    /// </summary>
    public interface IQueryParameterParser
    {
        /// <summary>
        /// Parses the parameters from the request query string.
        /// </summary>
        /// <param name="disableQueryAttribute">
        /// The <see cref="DisableQueryAttribute"/> if set on the controller that is targeted by the current request.
        /// </param>
        void Parse(DisableQueryAttribute disableQueryAttribute);
    }
}
