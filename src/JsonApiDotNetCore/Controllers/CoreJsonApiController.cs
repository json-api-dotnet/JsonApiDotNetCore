using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// Provides helper methods to raise JSON:API compliant errors from controller actions.
    /// </summary>
    public abstract class CoreJsonApiController : ControllerBase
    {
        protected IActionResult Error(ErrorObject error)
        {
            ArgumentGuard.NotNull(error, nameof(error));

            return Error(error.AsEnumerable());
        }

        protected IActionResult Error(IEnumerable<ErrorObject> errors)
        {
            ArgumentGuard.NotNull(errors, nameof(errors));

            var document = new ErrorDocument
            {
                Errors = errors.ToList()
            };

            return new ObjectResult(document)
            {
                StatusCode = (int)document.GetErrorStatusCode()
            };
        }
    }
}
