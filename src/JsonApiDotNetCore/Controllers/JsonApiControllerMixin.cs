using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models.JsonApiDocuments;
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
            return Errors(new[] {error});
        }

        protected IActionResult Errors(IEnumerable<Error> errors)
        {
            var document = new ErrorDocument(errors.ToList());

            return new ObjectResult(document)
            {
                StatusCode = (int) document.GetErrorStatusCode()
            };
        }
    }
}
