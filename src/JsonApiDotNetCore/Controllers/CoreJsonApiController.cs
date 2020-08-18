using System.Collections.Generic;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    [ServiceFilter(typeof(IQueryStringActionFilter))]
    public abstract class CoreJsonApiController : ControllerBase
    {
        protected IActionResult Error(Serialization.Objects.Error error)
        {
            return Error(new[] {error});
        }

        protected IActionResult Error(IEnumerable<Error> errors)
        {
            var document = new ErrorDocument(errors);

            return new ObjectResult(document)
            {
                StatusCode = (int) document.GetErrorStatusCode()
            };
        }
    }
}
