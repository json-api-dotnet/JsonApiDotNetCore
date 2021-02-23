using System.Collections.Generic;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// Provides helper methods to raise JSON:API compliant errors from controller actions.
    /// </summary>
    public abstract class CoreJsonApiController : ControllerBase
    {
        protected IActionResult Error(Error error)
        {
            ArgumentGuard.NotNull(error, nameof(error));

            return Error(error.AsEnumerable());
        }

        protected IActionResult Error(IEnumerable<Error> errors)
        {
            ArgumentGuard.NotNull(errors, nameof(errors));

            var document = new ErrorDocument(errors);

            return new ObjectResult(document)
            {
                StatusCode = (int)document.GetErrorStatusCode()
            };
        }
    }
}
