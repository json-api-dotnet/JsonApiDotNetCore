using System.Net;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    [ServiceFilter(typeof(IQueryParameterActionFilter))]
    public abstract class JsonApiControllerMixin : ControllerBase
    {
        protected IActionResult Forbidden()
        {
            return new StatusCodeResult((int)HttpStatusCode.Forbidden);
        }

        protected IActionResult Error(Error error)
        {
          return error.AsActionResult();
        }

        protected IActionResult Errors(ErrorCollection errors)
        {
          return errors.AsActionResult();
        }
    }
}
