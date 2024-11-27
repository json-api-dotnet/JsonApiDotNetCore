using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JsonApiDotNetCore.Middleware;

internal static class HttpMethodAttributeExtensions
{
    private const string IdTemplate = "{id}";
    private const string RelationshipNameTemplate = "{relationshipName}";
    private const string SecondaryEndpointTemplate = $"{IdTemplate}/{RelationshipNameTemplate}";
    private const string RelationshipEndpointTemplate = $"{IdTemplate}/relationships/{RelationshipNameTemplate}";

    public static JsonApiEndpoints GetJsonApiEndpoint(this IEnumerable<HttpMethodAttribute> httpMethods)
    {
        ArgumentGuard.NotNull(httpMethods);

        HttpMethodAttribute[] nonHeadAttributes = httpMethods.Where(attribute => attribute is not HttpHeadAttribute).ToArray();

        return nonHeadAttributes.Length == 1 ? ResolveJsonApiEndpoint(nonHeadAttributes[0]) : JsonApiEndpoints.None;
    }

    private static JsonApiEndpoints ResolveJsonApiEndpoint(HttpMethodAttribute httpMethod)
    {
        return httpMethod switch
        {
            HttpGetAttribute httpGet => httpGet.Template switch
            {
                null => JsonApiEndpoints.GetCollection,
                IdTemplate => JsonApiEndpoints.GetSingle,
                SecondaryEndpointTemplate => JsonApiEndpoints.GetSecondary,
                RelationshipEndpointTemplate => JsonApiEndpoints.GetRelationship,
                _ => JsonApiEndpoints.None
            },
            HttpPostAttribute httpPost => httpPost.Template switch
            {
                null => JsonApiEndpoints.Post,
                RelationshipEndpointTemplate => JsonApiEndpoints.PostRelationship,
                _ => JsonApiEndpoints.None
            },
            HttpPatchAttribute httpPatch => httpPatch.Template switch
            {
                IdTemplate => JsonApiEndpoints.Patch,
                RelationshipEndpointTemplate => JsonApiEndpoints.PatchRelationship,
                _ => JsonApiEndpoints.None
            },
            HttpDeleteAttribute httpDelete => httpDelete.Template switch
            {
                IdTemplate => JsonApiEndpoints.Delete,
                RelationshipEndpointTemplate => JsonApiEndpoints.DeleteRelationship,
                _ => JsonApiEndpoints.None
            },
            _ => JsonApiEndpoints.None
        };
    }
}
