using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class JsonApiControllerMixin : Controller
    {
        protected IActionResult UnprocessableEntity()
        {
            return new StatusCodeResult(422);
        }

        protected IActionResult Forbidden()
        {
            return new StatusCodeResult(403);
        }

        protected IActionResult Error(Error error)
        {
            var errorCollection = new ErrorCollection
            {
                Errors = new List<Error> { error }
            };
            var result = new ObjectResult(errorCollection);
            result.StatusCode = error.StatusCode;

            return result;
        }

        protected IActionResult Errors(ErrorCollection errors)
        {
            var result = new ObjectResult(errors);
            result.StatusCode = GetErrorStatusCode(errors);

            return result;
        }

        private int GetErrorStatusCode(ErrorCollection errors)
        {
            var statusCodes = errors.Errors
                .Select(e => e.StatusCode)
                .Distinct()
                .ToList();

            if (statusCodes.Count == 1)
                return statusCodes[0];

            return int.Parse(statusCodes.Max().ToString()[0] + "00");
        }
    }
}
