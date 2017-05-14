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

        protected IActionResult Errors(ErrorCollection errors)
        {
            var result = new ObjectResult(errors);
            result.StatusCode = GetErrorStatusCode(errors);

            return result;
        }

        private int GetErrorStatusCode(ErrorCollection errors) 
        {
            var statusCodes = errors.Errors
                .Select(e => (int)e.StatusCode)
                .Distinct()
                .ToList();

            if(statusCodes.Count == 1)
                return statusCodes[0];
            
            return int.Parse(statusCodes.Max().ToString()[0] + "00");
        }
    }
}
