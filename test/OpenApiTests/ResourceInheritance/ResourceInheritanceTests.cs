using System.Collections.ObjectModel;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable AV1755 // Name of async method should end with Async or TaskAsync

namespace OpenApiTests.ResourceInheritance;

public abstract class ResourceInheritanceTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;

    protected ResourceInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper, bool hasOperationsController, bool writeToDisk)
    {
        _testContext = testContext;

        testContext.UseInheritanceControllers(hasOperationsController);
        testContext.SetTestOutputHelper(testOutputHelper);

        if (writeToDisk)
        {
            testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
        }
    }

    public virtual async Task Only_expected_endpoints_are_exposed(Type resourceClrType, JsonApiEndpoints expected)
    {
        // Arrange
        var resourceGraph = _testContext.Factory.Services.GetRequiredService<IResourceGraph>();
        ResourceType resourceType = resourceGraph.GetResourceType(resourceClrType);
        IReadOnlyDictionary<JsonApiEndpoints, ReadOnlyCollection<string>> endpointToPathMap = JsonPathBuilder.GetEndpointPaths(resourceType);

        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string[] pathsExpected = JsonPathBuilder.KnownEndpoints.Where(endpoint => expected.HasFlag(endpoint))
            .SelectMany(endpoint => endpointToPathMap[endpoint]).ToArray();

        string[] pathsNotExpected = endpointToPathMap.Values.SelectMany(paths => paths).Except(pathsExpected).ToArray();

        foreach (string path in pathsExpected)
        {
            document.Should().ContainPath(path);
        }

        foreach (string path in pathsNotExpected)
        {
            document.Should().NotContainPath(path);
        }
    }

    public virtual async Task Operations_endpoint_is_exposed(bool enabled)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        if (enabled)
        {
            document.Should().ContainPath("paths./operations.post");
        }
        else
        {
            document.Should().NotContainPath("paths./operations.post");
        }
    }

    public virtual async Task Expected_names_appear_in_type_discriminator_mapping(string schemaName, bool isWrapped, string? discriminatorValues)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        if (discriminatorValues == null)
        {
            document.Should().NotContainPath($"components.schemas.{schemaName}");
        }
        else
        {
            string schemaPath = isWrapped ? $"components.schemas.{schemaName}.allOf[1]" : $"components.schemas.{schemaName}";

            document.Should().ContainPath(schemaPath).With(schemaElement =>
            {
                schemaElement.Should().ContainPath("discriminator").With(discriminatorElement =>
                {
                    discriminatorElement.Should().HaveProperty("propertyName", "type");

                    if (discriminatorValues.Length == 0)
                    {
                        discriminatorElement.Should().NotContainPath("mapping");
                    }
                    else
                    {
                        discriminatorElement.Should().ContainPath("mapping").With(mappingElement =>
                        {
                            string[] valueArray = discriminatorValues.Split('|');
                            mappingElement.EnumerateObject().Should().HaveCount(valueArray.Length);

                            foreach (string value in valueArray)
                            {
                                mappingElement.Should().ContainProperty(value);
                            }
                        });
                    }
                });
            });
        }
    }

    public virtual async Task Expected_names_appear_in_openapi_discriminator_mapping(string prefixedSchemaName, string? discriminatorValues)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        string schemaName = prefixedSchemaName.StartsWith('!') ? prefixedSchemaName[1..] : prefixedSchemaName;
        string discriminatorPath = prefixedSchemaName.StartsWith('!') ? "allOf[1].discriminator" : "discriminator";

        // Assert
        if (discriminatorValues == null)
        {
            document.Should().NotContainPath($"components.schemas.{schemaName}");
        }
        else
        {
            document.Should().ContainPath($"components.schemas.{schemaName}").With(schemaElement =>
            {
                schemaElement.Should().ContainPath(discriminatorPath).With(discriminatorElement =>
                {
                    discriminatorElement.Should().HaveProperty("propertyName", "openapi:discriminator");

                    if (discriminatorValues.Length == 0)
                    {
                        discriminatorElement.Should().NotContainPath("mapping");
                    }
                    else
                    {
                        discriminatorElement.Should().ContainPath("mapping").With(mappingElement =>
                        {
                            string[] valueArray = discriminatorValues.Split('|');
                            mappingElement.EnumerateObject().Should().HaveCount(valueArray.Length);

                            foreach (string value in valueArray)
                            {
                                mappingElement.Should().ContainProperty(value);
                            }
                        });
                    }
                });
            });
        }
    }

    public virtual async Task Expected_names_appear_in_resource_type_enum(string schemaName, string? enumValues)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        if (enumValues == null)
        {
            document.Should().NotContainPath($"components.schemas.{schemaName}");
        }
        else
        {
            document.Should().ContainPath($"components.schemas.{schemaName}").With(schemaElement =>
            {
                if (enumValues.Length == 0)
                {
                    schemaElement.Should().NotContainPath("enum");
                }
                else
                {
                    schemaElement.Should().ContainPath("enum").With(enumElement =>
                    {
                        string[] valueArray = enumValues.Split('|');
                        enumElement.EnumerateArray().Should().HaveCount(valueArray.Length);

                        foreach (string value in valueArray)
                        {
                            enumElement.Should().ContainArrayElement(value);
                        }
                    });
                }
            });
        }
    }

    public virtual async Task Component_schemas_have_expected_base_type(string schemaName, bool isAbstract, string? baseType, string? properties)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        if (baseType == null && properties == null)
        {
            document.Should().NotContainPath($"components.schemas.{schemaName}");
        }
        else
        {
            document.Should().ContainPath($"components.schemas.{schemaName}").With(schemaElement =>
            {
                if (baseType == null)
                {
                    schemaElement.Should().NotContainPath("allOf[0]");
                }
                else
                {
                    schemaElement.Should().HaveProperty("allOf[0].$ref", $"#/components/schemas/{baseType}");
                }

                string propertiesPath = baseType != null ? "allOf[1].properties" : "properties";

                if (properties == null)
                {
                    schemaElement.Should().NotContainPath(propertiesPath);
                }
                else
                {
                    string[] propertyArray = properties.Split('|');

                    schemaElement.Should().ContainPath(propertiesPath).With(propertiesElement =>
                    {
                        propertiesElement.EnumerateObject().Should().HaveCount(propertyArray.Length);

                        foreach (string value in propertyArray)
                        {
                            propertiesElement.Should().ContainProperty(value);
                        }
                    });
                }

                string abstractPath = baseType != null ? "allOf[1].x-abstract" : "x-abstract";

                if (!isAbstract)
                {
                    schemaElement.Should().NotContainPath(abstractPath);
                }
                else
                {
                    schemaElement.Should().ContainPath(abstractPath).With(abstractElement =>
                    {
                        abstractElement.Should().Be(true);
                    });
                }
            });
        }
    }
}
