using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Action filter used to verify the incoming type matches the target type, else return a 409
    /// </summary>
    public interface IJsonApiTypeMatchFilter : IActionFilter { }
}
