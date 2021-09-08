using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class EndpointResolver
    {
        public JsonApiEndpoint? Get(MethodInfo controllerAction)
        {
            ArgumentGuard.NotNull(controllerAction, nameof(controllerAction));

            HttpMethodAttribute method = controllerAction.GetCustomAttributes(true).OfType<HttpMethodAttribute>().FirstOrDefault();

            return ResolveJsonApiEndpoint(method);
        }

        private static JsonApiEndpoint? ResolveJsonApiEndpoint(HttpMethodAttribute httpMethod)
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
