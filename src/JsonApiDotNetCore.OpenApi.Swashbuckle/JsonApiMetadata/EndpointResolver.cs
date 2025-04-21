using System.Reflection;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class EndpointResolver
{
    public static EndpointResolver Instance { get; } = new();

    private EndpointResolver()
    {
    }

    public JsonApiEndpoints GetEndpoint(MethodInfo controllerAction)
    {
        ArgumentNullException.ThrowIfNull(controllerAction);

        if (!IsJsonApiController(controllerAction))
        {
            return JsonApiEndpoints.None;
        }

        IEnumerable<HttpMethodAttribute> httpMethodAttributes = controllerAction.GetCustomAttributes<HttpMethodAttribute>(true);
        return httpMethodAttributes.GetJsonApiEndpoint();
    }

    private bool IsJsonApiController(MethodInfo controllerAction)
    {
        return typeof(CoreJsonApiController).IsAssignableFrom(controllerAction.ReflectedType);
    }

    public bool IsAtomicOperationsController(MethodInfo controllerAction)
    {
        return typeof(BaseJsonApiOperationsController).IsAssignableFrom(controllerAction.ReflectedType);
    }
}
