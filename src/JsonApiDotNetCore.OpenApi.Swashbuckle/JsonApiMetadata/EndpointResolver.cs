using System.Reflection;
using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class EndpointResolver
{
    public JsonApiEndpoint? Get(MethodInfo controllerAction)
    {
        ArgumentGuard.NotNull(controllerAction);

        if (!IsJsonApiController(controllerAction))
        {
            return null;
        }

        if (IsAtomicOperationsController(controllerAction))
        {
            return JsonApiEndpoint.PostOperations;
        }

        HttpMethodAttribute? method = Attribute.GetCustomAttributes(controllerAction, true).OfType<HttpMethodAttribute>().FirstOrDefault();

        return ResolveJsonApiEndpoint(method);
    }

    private static bool IsJsonApiController(MethodInfo controllerAction)
    {
        return typeof(CoreJsonApiController).IsAssignableFrom(controllerAction.ReflectedType);
    }

    private static bool IsAtomicOperationsController(MethodInfo controllerAction)
    {
        return typeof(BaseJsonApiOperationsController).IsAssignableFrom(controllerAction.ReflectedType);
    }

    private static JsonApiEndpoint? ResolveJsonApiEndpoint(HttpMethodAttribute? httpMethod)
    {
        return httpMethod switch
        {
            HttpGetAttribute attr => attr.Template switch
            {
                null => JsonApiEndpoint.GetCollection,
                JsonApiRoutingTemplate.PrimaryEndpoint => JsonApiEndpoint.GetSingle,
                JsonApiRoutingTemplate.SecondaryEndpoint => JsonApiEndpoint.GetSecondary,
                JsonApiRoutingTemplate.RelationshipEndpoint => JsonApiEndpoint.GetRelationship,
                _ => null
            },
            HttpPostAttribute attr => attr.Template switch
            {
                null => JsonApiEndpoint.PostResource,
                JsonApiRoutingTemplate.RelationshipEndpoint => JsonApiEndpoint.PostRelationship,
                _ => null
            },
            HttpPatchAttribute attr => attr.Template switch
            {
                JsonApiRoutingTemplate.PrimaryEndpoint => JsonApiEndpoint.PatchResource,
                JsonApiRoutingTemplate.RelationshipEndpoint => JsonApiEndpoint.PatchRelationship,
                _ => null
            },
            HttpDeleteAttribute attr => attr.Template switch
            {
                JsonApiRoutingTemplate.PrimaryEndpoint => JsonApiEndpoint.DeleteResource,
                JsonApiRoutingTemplate.RelationshipEndpoint => JsonApiEndpoint.DeleteRelationship,
                _ => null
            },
            _ => null
        };
    }
}
