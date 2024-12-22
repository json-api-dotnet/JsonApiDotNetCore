using System.Collections.ObjectModel;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers;

/// <summary>
/// Provides helper methods to raise JSON:API compliant errors from controller actions.
/// </summary>
public abstract class CoreJsonApiController : ControllerBase
{
    protected IActionResult Error(ErrorObject error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ObjectResult(error)
        {
            StatusCode = (int)error.StatusCode
        };
    }

    protected IActionResult Error(IEnumerable<ErrorObject> errors)
    {
        ReadOnlyCollection<ErrorObject>? errorCollection = ToCollection(errors);
        ArgumentGuard.NotNullNorEmpty(errorCollection, nameof(errors));

        return new ObjectResult(errorCollection)
        {
            StatusCode = (int)ErrorObject.GetResponseStatusCode(errorCollection)
        };
    }

    private static ReadOnlyCollection<ErrorObject>? ToCollection(IEnumerable<ErrorObject>? errors)
    {
        return errors?.ToArray().AsReadOnly();
    }
}
