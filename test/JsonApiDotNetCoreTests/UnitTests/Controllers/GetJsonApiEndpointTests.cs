using FluentAssertions;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Controllers;

public sealed class GetJsonApiEndpointTests
{
    [Theory]
    [InlineData("GET", null, JsonApiEndpoints.GetCollection)]
    [InlineData("GET", "{id}", JsonApiEndpoints.GetSingle)]
    [InlineData("GET", "{id}/{relationshipName}", JsonApiEndpoints.GetSecondary)]
    [InlineData("GET", "{id}/relationships/{relationshipName}", JsonApiEndpoints.GetRelationship)]
    [InlineData("POST", null, JsonApiEndpoints.Post)]
    [InlineData("POST", "{id}/relationships/{relationshipName}", JsonApiEndpoints.PostRelationship)]
    [InlineData("PATCH", "{id}", JsonApiEndpoints.Patch)]
    [InlineData("PATCH", "{id}/relationships/{relationshipName}", JsonApiEndpoints.PatchRelationship)]
    [InlineData("DELETE", "{id}", JsonApiEndpoints.Delete)]
    [InlineData("DELETE", "{id}/relationships/{relationshipName}", JsonApiEndpoints.DeleteRelationship)]
    [InlineData("PUT", null, JsonApiEndpoints.None)]
    public void Can_identify_endpoint_from_http_method_and_route_template(string httpMethod, string? routeTemplate, JsonApiEndpoints expected)
    {
        // Arrange
        HttpMethodAttribute attribute = httpMethod switch
        {
            "GET" => routeTemplate == null ? new HttpGetAttribute() : new HttpGetAttribute(routeTemplate),
            "POST" => routeTemplate == null ? new HttpPostAttribute() : new HttpPostAttribute(routeTemplate),
            "PATCH" => routeTemplate == null ? new HttpPatchAttribute() : new HttpPatchAttribute(routeTemplate),
            "DELETE" => routeTemplate == null ? new HttpDeleteAttribute() : new HttpDeleteAttribute(routeTemplate),
            "PUT" => routeTemplate == null ? new HttpPutAttribute() : new HttpPutAttribute(routeTemplate),
            _ => throw new ArgumentOutOfRangeException(nameof(httpMethod), httpMethod, null)
        };

        // Act
        JsonApiEndpoints endpoint = HttpMethodAttributeExtensions.GetJsonApiEndpoint([attribute]);

        // Assert
        endpoint.Should().Be(expected);
    }
}
