using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal.QueryStrings;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Entry point for processing all query string parameters.
    /// </summary>
    public interface IQueryStringActionFilter : IAsyncActionFilter { }
}
