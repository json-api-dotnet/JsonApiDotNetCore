using System;
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
            if (error == null) throw new ArgumentNullException(nameof(error));

            return Error(new[] {error});
        }

        protected IActionResult Error(IEnumerable<Error> errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));

            var document = new ErrorDocument(errors);

            return new ObjectResult(document)
            {
                StatusCode = (int) document.GetErrorStatusCode()
            };
        }
    }
}
