using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.MixedControllers;

public sealed class MixedControllerTests : IClassFixture<OpenApiTestContext<MixedControllerStartup, CoffeeDbContext>>
{
    private readonly OpenApiTestContext<MixedControllerStartup, CoffeeDbContext> _testContext;

    public MixedControllerTests(OpenApiTestContext<MixedControllerStartup, CoffeeDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<CupOfCoffeesController>();
        testContext.UseController<CoffeeSummaryController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.OpenApiDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Fact]
    public async Task Default_JsonApi_endpoints_are_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./cupOfCoffees.get");
        document.Should().ContainPath("paths./cupOfCoffees.head");
        document.Should().ContainPath("paths./cupOfCoffees/{id}.delete");
    }

    [Fact]
    public async Task Get_coffee_summaries_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./coffeeSummaries/summary.get").Should().BeJson("""
            {
              "tags": [
                "coffeeSummaries"
              ],
              "description": "Summarizes all cups of coffee, indicating their ingredients.",
              "operationId": "get-coffee-summary",
              "responses": {
                "200": {
                  "description": "Successfully returns the coffee summary.",
                  "content": {
                    "application/vnd.api+json; ext=\"https://www.jsonapi.net/ext/openapi\"": {
                      "schema": {
                        "$ref": "#/components/schemas/primaryCoffeeSummaryResponseDocument"
                      }
                    }
                  }
                },
                "404": {
                  "description": "The coffee summary does not exist.",
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
            """);

        document.Should().ContainPath("paths./coffeeSummaries/summary.head").Should().BeJson("""
            {
              "tags": [
                "coffeeSummaries"
              ],
              "description": "Summarizes all cups of coffee, indicating their ingredients.",
              "operationId": "head-coffee-summary",
              "responses": {
                "200": {
                  "description": "Successfully returns the coffee summary."
                },
                "404": {
                  "description": "The coffee summary does not exist."
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Get_black_cups_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./cupOfCoffees/onlyBlack.get").Should().BeJson("""
            {
              "tags": [
                "cupOfCoffees"
              ],
              "description": "Gets all cups of coffee without sugar and milk.",
              "operationId": "get-only-black",
              "responses": {
                "200": {
                  "description": "Successfully returns the cups of black coffee, or an empty array if none were found.",
                  "content": {
                    "application/vnd.api+json; ext=\"https://www.jsonapi.net/ext/openapi\"": {
                      "schema": {
                        "$ref": "#/components/schemas/cupOfCoffeeCollectionResponseDocument"
                      }
                    }
                  }
                }
              }
            }
            """);

        document.Should().ContainPath("paths./cupOfCoffees/onlyBlack.head").Should().BeJson("""
            {
              "tags": [
                "cupOfCoffees"
              ],
              "description": "Gets all cups of coffee without sugar and milk.",
              "operationId": "head-only-black",
              "responses": {
                "200": {
                  "description": "Successfully returns the cups of black coffee, or an empty array if none were found."
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Get_black_cup_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./cupOfCoffees/onlyBlack/{id}.get").Should().BeJson("""
            {
              "tags": [
                "cupOfCoffees"
              ],
              "description": "Gets a cup of coffee by ID, if the cup is without sugar and milk. Returns 404 otherwise.",
              "operationId": "get-only-if-black",
              "parameters": [
                {
                  "name": "id",
                  "in": "path",
                  "required": true,
                  "schema": {
                    "minLength": 1,
                    "type": "string",
                    "format": "int64"
                  }
                }
              ],
              "responses": {
                "200": {
                  "description": "Successfully returns the cup of black coffee.",
                  "content": {
                    "application/vnd.api+json; ext=\"https://www.jsonapi.net/ext/openapi\"": {
                      "schema": {
                        "$ref": "#/components/schemas/primaryCupOfCoffeeResponseDocument"
                      }
                    }
                  }
                },
                "404": {
                  "description": "The cup of coffee does not exist or is not black.",
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
            """);

        document.Should().ContainPath("paths./cupOfCoffees/onlyBlack/{id}.head").Should().BeJson("""
            {
              "tags": [
                "cupOfCoffees"
              ],
              "description": "Gets a cup of coffee by ID, if the cup is without sugar and milk. Returns 404 otherwise.",
              "operationId": "head-only-if-black",
              "parameters": [
                {
                  "name": "id",
                  "in": "path",
                  "required": true,
                  "schema": {
                    "minLength": 1,
                    "type": "string",
                    "format": "int64"
                  }
                }
              ],
              "responses": {
                "200": {
                  "description": "Successfully returns the cup of black coffee."
                },
                "404": {
                  "description": "The cup of coffee does not exist or is not black."
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Batch_create_cups_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./cupOfCoffees/batch.post").Should().BeJson("""
            {
              "tags": [
                "cupOfCoffees"
              ],
              "description": "Creates cups of coffee in batch.",
              "operationId": "batchCreateCupsOfCoffee",
              "parameters": [
                {
                  "name": "size",
                  "in": "query",
                  "description": "The batch size.",
                  "required": true,
                  "schema": {
                    "type": "integer",
                    "description": "The batch size.",
                    "format": "int32"
                  }
                }
              ],
              "requestBody": {
                "content": {
                  "application/vnd.api+json; ext=\"https://www.jsonapi.net/ext/openapi\"": {
                    "schema": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/createCupOfCoffeeRequestDocument"
                        }
                      ]
                    }
                  }
                },
                "required": true
              },
              "responses": {
                "204": {
                  "description": "All cups of coffee have been created successfully."
                },
                "400": {
                  "description": "Invalid batch size.",
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
            """);
    }

    [Fact]
    public async Task Batch_update_cups_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./cupOfCoffees/batch.patch").Should().BeJson("""
            {
              "tags": [
                "cupOfCoffees"
              ],
              "description": "Resets all cups of coffee to black.",
              "operationId": "batchResetToBlack",
              "responses": {
                "204": {
                  "description": "All cups of coffee have been reset to black."
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Batch_delete_cups_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./cupOfCoffees/batch.delete").Should().BeJson("""
            {
              "tags": [
                "cupOfCoffees"
              ],
              "description": "Deletes all cups of coffee. Returns 404 when none found.",
              "operationId": "deleteAll",
              "responses": {
                "204": {
                  "description": "All cups of coffee have been deleted."
                },
                "404": {
                  "description": "No cups of coffee were found.",
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
            """);
    }
}
