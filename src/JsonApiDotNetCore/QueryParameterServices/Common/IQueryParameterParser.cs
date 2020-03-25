using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Query;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Responsible for populating the various service implementations of
    /// <see cref="IQueryParameterService"/>.
    /// </summary>
    public interface IQueryParameterParser
    {
        void Parse(IQueryCollection query, DisableQueryAttribute disabledQuery = null);
    }
}
