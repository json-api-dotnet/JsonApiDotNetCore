using System.Reflection;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

internal abstract class OpenApiActionMethod
{
    public static OpenApiActionMethod Create(ActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        MethodInfo actionMethod = descriptor.GetActionMethod();

        if (IsJsonApiController(actionMethod))
        {
            Type? controllerType = actionMethod.ReflectedType;
            ConsistencyGuard.ThrowIf(controllerType == null);

            if (IsAtomicOperationsController(actionMethod))
            {
                var httpPostAttribute = actionMethod.GetCustomAttribute<HttpPostAttribute>(true);

                if (httpPostAttribute != null)
                {
                    return new AtomicOperationsActionMethod(controllerType);
                }
            }
            else
            {
                IEnumerable<HttpMethodAttribute> httpMethodAttributes = actionMethod.GetCustomAttributes<HttpMethodAttribute>(true);
                JsonApiEndpoints endpoint = httpMethodAttributes.GetJsonApiEndpoint();

                if (endpoint != JsonApiEndpoints.None)
                {
                    return new JsonApiActionMethod(endpoint, controllerType);
                }
            }

            return CustomJsonApiActionMethod.Instance;
        }

        return CustomControllerActionMethod.Instance;
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
