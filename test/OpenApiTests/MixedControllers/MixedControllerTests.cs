using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        testContext.UseController<FileTransferController>();
        testContext.UseController<CupOfCoffeesController>();
        testContext.UseController<CoffeeSummaryController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<InMemoryFileStorage>();
            services.AddSingleton<InMemoryOutgoingEmailsProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, MinimalApiStartupFilter>());
        });
    }

    [Fact]
    public async Task Default_JsonApi_endpoints_are_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./cupOfCoffees.get");
        document.Should().ContainPath("paths./cupOfCoffees.head");
        document.Should().ContainPath("paths./cupOfCoffees/{id}.delete");
    }

    [Fact]
    public async Task Upload_file_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./fileTransfers.post").Should().BeJson("""
            {
              "tags": [
                "fileTransfers"
              ],
              "description": "Uploads a file. Returns HTTP 400 if the file is empty.",
              "operationId": "upload",
              "requestBody": {
                "content": {
                  "multipart/form-data": {
                    "schema": {
                      "type": "object",
                      "properties": {
                        "file": {
                          "type": "string",
                          "format": "binary"
                        }
                      }
                    },
                    "encoding": {
                      "file": {
                        "style": "form"
                      }
                    }
                  }
                }
              },
              "responses": {
                "200": {
                  "description": "OK",
                  "content": {
                    "text/plain": {
                      "schema": {
                        "type": "string"
                      }
                    }
                  }
                },
                "400": {
                  "description": "Bad Request"
                }
              }
            }
            """);
    }

    [Fact]
    public async Task File_exists_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./fileTransfers/find.get").Should().BeJson("""
            {
              "tags": [
                "fileTransfers"
              ],
              "description": "Returns whether the specified file is available for download.",
              "operationId": "exists",
              "parameters": [
                {
                  "name": "fileName",
                  "in": "query",
                  "schema": {
                    "type": "string"
                  }
                }
              ],
              "responses": {
                "200": {
                  "description": "OK"
                },
                "404": {
                  "description": "Not Found"
                }
              }
            }
            """);

        document.Should().ContainPath("paths./fileTransfers/find.head").Should().BeJson("""
            {
              "tags": [
                "fileTransfers"
              ],
              "description": "Returns whether the specified file is available for download.",
              "operationId": "tryExists",
              "parameters": [
                {
                  "name": "fileName",
                  "in": "query",
                  "schema": {
                    "type": "string"
                  }
                }
              ],
              "responses": {
                "200": {
                  "description": "OK"
                },
                "404": {
                  "description": "Not Found"
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Download_file_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./fileTransfers.get").Should().BeJson("""
            {
              "tags": [
                "fileTransfers"
              ],
              "description": "Downloads the file with the specified name. Returns HTTP 404 if not found.",
              "operationId": "download",
              "parameters": [
                {
                  "name": "fileName",
                  "in": "query",
                  "schema": {
                    "type": "string"
                  }
                }
              ],
              "responses": {
                "200": {
                  "description": "OK",
                  "content": {
                    "application/octet-stream": {
                      "schema": {
                        "type": "string",
                        "format": "binary"
                      }
                    }
                  }
                },
                "404": {
                  "description": "Not Found"
                }
              }
            }
            """);

        document.Should().ContainPath("paths./fileTransfers.head").Should().BeJson("""
            {
              "tags": [
                "fileTransfers"
              ],
              "description": "Downloads the file with the specified name. Returns HTTP 404 if not found.",
              "operationId": "tryDownload",
              "parameters": [
                {
                  "name": "fileName",
                  "in": "query",
                  "schema": {
                    "type": "string"
                  }
                }
              ],
              "responses": {
                "200": {
                  "description": "OK"
                },
                "404": {
                  "description": "Not Found"
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Send_email_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./emails/send.post").Should().BeJson("""
            {
              "tags": [
                "emails"
              ],
              "description": "Sends an email to the specified recipient.",
              "operationId": "sendEmail",
              "requestBody": {
                "content": {
                  "application/json": {
                    "schema": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/email"
                        }
                      ],
                      "description": "The email to send."
                    }
                  }
                },
                "required": true
              },
              "responses": {
                "200": {
                  "description": "OK"
                },
                "400": {
                  "description": "Bad Request",
                  "content": {
                    "application/problem+json": {
                      "schema": {
                        "$ref": "#/components/schemas/httpValidationProblemDetails"
                      }
                    }
                  }
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Emails_sent_since_endpoint_is_exposed()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./emails/sent-since.get").Should().BeJson("""
            {
              "tags": [
                "emails"
              ],
              "description": "Gets all emails sent since the specified date/time.",
              "operationId": "getSentSince",
              "parameters": [
                {
                  "name": "sinceUtc",
                  "in": "query",
                  "description": "The date/time (in UTC) since which the email was sent.",
                  "required": true,
                  "schema": {
                    "type": "string",
                    "description": "The date/time (in UTC) since which the email was sent.",
                    "format": "date-time"
                  }
                }
              ],
              "responses": {
                "200": {
                  "description": "OK",
                  "content": {
                    "application/json": {
                      "schema": {
                        "type": "array",
                        "items": {
                          "$ref": "#/components/schemas/email"
                        }
                      }
                    }
                  }
                },
                "400": {
                  "description": "Bad Request",
                  "content": {
                    "application/problem+json": {
                      "schema": {
                        "$ref": "#/components/schemas/httpValidationProblemDetails"
                      }
                    }
                  }
                }
              }
            }
            """);

        document.Should().ContainPath("paths./emails/sent-since.head").Should().BeJson("""
            {
              "tags": [
                "emails"
              ],
              "description": "Gets all emails sent since the specified date/time.",
              "operationId": "tryGetSentSince",
              "parameters": [
                {
                  "name": "sinceUtc",
                  "in": "query",
                  "description": "The date/time (in UTC) since which the email was sent.",
                  "required": true,
                  "schema": {
                    "type": "string",
                    "description": "The date/time (in UTC) since which the email was sent.",
                    "format": "date-time"
                  }
                }
              ],
              "responses": {
                "200": {
                  "description": "OK"
                },
                "400": {
                  "description": "Bad Request"
                }
              }
            }
            """);
    }
}
