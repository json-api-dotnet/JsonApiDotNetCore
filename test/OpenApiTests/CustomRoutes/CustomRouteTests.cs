using System.Collections.ObjectModel;
using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.CustomRoutes;

public sealed class CustomRouteTests : IClassFixture<OpenApiTestContext<OpenApiStartup<CustomRouteDbContext>, CustomRouteDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<CustomRouteDbContext>, CustomRouteDbContext> _testContext;

    public CustomRouteTests(OpenApiTestContext<OpenApiStartup<CustomRouteDbContext>, CustomRouteDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<ElectionsController>();
        testContext.UseController<CandidatesController>();
        testContext.UseController<BallotsController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.OpenApiDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [InlineData(typeof(Election), "voting-api/overview")]
    [InlineData(typeof(Candidate), "voting-api/contenders")]
    [InlineData(typeof(Ballot), "voting-api/votes")]
    public async Task Only_expected_endpoints_are_exposed(Type resourceClrType, string routeTemplate)
    {
        // Arrange
        var resourceGraph = _testContext.Factory.Services.GetRequiredService<IResourceGraph>();
        ResourceType resourceType = resourceGraph.GetResourceType(resourceClrType);

        IReadOnlyDictionary<JsonApiEndpoints, ReadOnlyCollection<string>> customEndpointToPathMap =
            JsonPathBuilder.GetEndpointPaths(routeTemplate, resourceType.Relationships);

        IReadOnlyDictionary<JsonApiEndpoints, ReadOnlyCollection<string>> defaultEndpointToPathMap = JsonPathBuilder.GetEndpointPaths(resourceType);

        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        string[] customPaths = JsonPathBuilder.KnownEndpoints.SelectMany(endpoint => customEndpointToPathMap[endpoint]).ToArray();
        string[] defaultPaths = JsonPathBuilder.KnownEndpoints.SelectMany(endpoint => defaultEndpointToPathMap[endpoint]).ToArray();

        foreach (string path in customPaths)
        {
            document.Should().ContainPath(path);
        }

        foreach (string path in defaultPaths)
        {
            document.Should().NotContainPath(path);
        }
    }

    [Fact]
    public async Task Winner_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./voting-api/overview/winner/{id}").Should().BeJson("""
            {
              "head": {
                "tags": [
                  "elections"
                ],
                "summary": "Gets the candidate with the most votes for a given election.",
                "operationId": "head-winner",
                "parameters": [
                  {
                    "name": "id",
                    "in": "path",
                    "description": "The identifier of the election.",
                    "required": true,
                    "schema": {
                      "minLength": 1,
                      "type": "string",
                      "format": "uuid"
                    }
                  }
                ],
                "responses": {
                  "200": {
                    "description": "Successfully returns the winner."
                  },
                  "404": {
                    "description": "The election does not exist."
                  },
                  "409": {
                    "description": "No single winner found."
                  }
                }
              },
              "get": {
                "tags": [
                  "elections"
                ],
                "summary": "Gets the candidate with the most votes for a given election.",
                "operationId": "get-winner",
                "parameters": [
                  {
                    "name": "id",
                    "in": "path",
                    "description": "The identifier of the election.",
                    "required": true,
                    "schema": {
                      "minLength": 1,
                      "type": "string",
                      "format": "uuid"
                    }
                  }
                ],
                "responses": {
                  "200": {
                    "description": "Successfully returns the winner.",
                    "content": {
                      "application/vnd.api+json; ext=\"https://www.jsonapi.net/ext/openapi\"": {
                        "schema": {
                          "$ref": "#/components/schemas/primaryCandidateResponseDocument"
                        }
                      }
                    }
                  },
                  "404": {
                    "description": "The election does not exist.",
                    "content": {
                      "application/vnd.api+json; ext=\"https://www.jsonapi.net/ext/openapi\"": {
                        "schema": {
                          "$ref": "#/components/schemas/errorResponseDocument"
                        }
                      }
                    }
                  },
                  "409": {
                    "description": "No single winner found.",
                    "content": {
                      "application/vnd.api+json; ext=\"https://www.jsonapi.net/ext/openapi\"": {
                        "schema": {
                          "$ref": "#/components/schemas/errorResponseDocument"
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
    }
}
