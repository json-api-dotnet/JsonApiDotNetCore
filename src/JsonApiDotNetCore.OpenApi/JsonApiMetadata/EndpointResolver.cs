using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class EndpointResolver
    {
        public JsonApiEndpoint? Get(MethodInfo controllerAction)
        {
            ArgumentGuard.NotNull(controllerAction, nameof(controllerAction));

            // This is a temporary work-around to prevent the JsonApiDotNetCoreExample project from crashing upon startup.
            if (!IsJsonApiController(controllerAction) || IsOperationsController(controllerAction))
            {
                return null;
            }

            HttpMethodAttribute? method = controllerAction.GetCustomAttributes(true).OfType<HttpMethodAttribute>().FirstOrDefault();

            return ResolveJsonApiEndpoint(method);
        }

        private static bool IsJsonApiController(MethodInfo controllerAction)
        {
            return typeof(CoreJsonApiController).IsAssignableFrom(controllerAction.ReflectedType);
        }

        private static bool IsOperationsController(MethodInfo controllerAction)
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
                    null => JsonApiEndpoint.Post,
                    JsonApiRoutingTemplate.RelationshipEndpoint => JsonApiEndpoint.PostRelationship,
                    _ => null
                },
                HttpPatchAttribute attr => attr.Template switch
                {
                    JsonApiRoutingTemplate.PrimaryEndpoint => JsonApiEndpoint.Patch,
                    JsonApiRoutingTemplate.RelationshipEndpoint => JsonApiEndpoint.PatchRelationship,
                    _ => null
                },
                HttpDeleteAttribute attr => attr.Template switch
                {
                    JsonApiRoutingTemplate.PrimaryEndpoint => JsonApiEndpoint.Delete,
                    JsonApiRoutingTemplate.RelationshipEndpoint => JsonApiEndpoint.DeleteRelationship,
                    _ => null
                },
                _ => null
            };
        }
    }
}
