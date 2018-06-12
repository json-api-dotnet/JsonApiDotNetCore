using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class JsonApiControllerMixin : ControllerBase
    {
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

            return new ObjectResult(errorCollection)
            {
                StatusCode = error.StatusCode
            };
        }

        protected IActionResult Errors(ErrorCollection errors)
        {
            return new ObjectResult(errors)
            {
                StatusCode = GetErrorStatusCode(errors)
            };
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
