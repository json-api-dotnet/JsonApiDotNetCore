using System.Reflection;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// A JSON:API controller-based action method.
/// </summary>
internal abstract class JsonApiActionMethod
{
    public Type ControllerType { get; }

    protected JsonApiActionMethod(Type controllerType)
    {
        ArgumentNullException.ThrowIfNull(controllerType);

        ControllerType = controllerType;
    }

    public static JsonApiActionMethod? TryCreate(ActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        MethodInfo? actionMethod = descriptor.TryGetActionMethod();

        if (actionMethod != null)
        {
            if (IsJsonApiController(actionMethod))
            {
                Type? controllerType = actionMethod.ReflectedType;
                ConsistencyGuard.ThrowIf(controllerType == null);

                if (IsAtomicOperationsController(actionMethod))
                {
                    var httpPostAttribute = actionMethod.GetCustomAttribute<HttpPostAttribute>(true);

                    if (httpPostAttribute != null)
                    {
                        return new OperationsActionMethod(controllerType);
                    }
                }
                else
                {
                    IEnumerable<HttpMethodAttribute> httpMethodAttributes = actionMethod.GetCustomAttributes<HttpMethodAttribute>(true);
                    JsonApiEndpoints endpoint = httpMethodAttributes.GetJsonApiEndpoint();

                    if (endpoint != JsonApiEndpoints.None)
                    {
                        return new BuiltinResourceActionMethod(endpoint, controllerType);
                    }
                }

                return new CustomResourceActionMethod(descriptor, controllerType);
            }
        }

        // An action method in a custom controller or a Minimal API endpoint, unrelated to JSON:API.
        return null;
    }

    private static bool IsJsonApiController(MethodInfo controllerAction)
    {
        return typeof(CoreJsonApiController).IsAssignableFrom(controllerAction.ReflectedType);
    }

    private static bool IsAtomicOperationsController(MethodInfo controllerAction)
    {
        return typeof(BaseJsonApiOperationsController).IsAssignableFrom(controllerAction.ReflectedType);
    }
}
