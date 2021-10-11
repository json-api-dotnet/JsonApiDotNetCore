#nullable disable

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

            return new ObjectResult(error)
            {
                StatusCode = (int)error.StatusCode
            };
        }

        protected IActionResult Error(IEnumerable<ErrorObject> errors)
        {
            ArgumentGuard.NotNull(errors, nameof(errors));

            ErrorObject[] errorArray = errors.ToArray();

            return new ObjectResult(errorArray)
            {
                StatusCode = (int)ErrorObject.GetResponseStatusCode(errorArray)
            };
        }
    }
}
