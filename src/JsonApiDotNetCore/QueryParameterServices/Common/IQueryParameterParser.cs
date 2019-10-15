using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Responsible for populating the various service implementations of
    /// <see cref="IQueryParameterService"/>.
    /// </summary>
    public interface IQueryParameterDiscovery
    {
        void Parse(IQueryCollection query, DisableQueryAttribute disabledQuery = null);
    }
}
