using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Meta
{
    public sealed class AtomicResponseMetaTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new();

        public AtomicResponseMetaTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OperationsController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<ImplicitlyChangingTextLanguageDefinition>();

                services.AddSingleton<ResourceDefinitionHitCounter>();
                services.AddSingleton<IResponseMeta, AtomicResponseMeta>();
            });
        }

        [Fact]
        public async Task Returns_top_level_meta_in_create_resource_with_side_effects()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.ShouldHaveCount(3);

            responseDocument.Meta.ShouldContainKey("license").With(value =>
            {
                JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be("MIT");
            });

            responseDocument.Meta.ShouldContainKey("projectUrl").With(value =>
            {
                JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be("https://github.com/json-api-dotnet/JsonApiDotNetCore/");
            });

            responseDocument.Meta.ShouldContainKey("versions").With(value =>
            {
                JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                string?[] versionArray = element.EnumerateArray().Select(arrayItem => arrayItem.GetString()).ToArray();

                versionArray.ShouldHaveCount(4);
                versionArray.Should().Contain("v4.0.0");
                versionArray.Should().Contain("v3.1.0");
                versionArray.Should().Contain("v2.5.2");
                versionArray.Should().Contain("v1.3.1");
            });
        }

        [Fact]
        public async Task Returns_top_level_meta_in_update_resource_with_side_effects()
        {
            // Arrange
            TextLanguage existingLanguage = _fakers.TextLanguage.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TextLanguages.Add(existingLanguage);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "textLanguages",
                            id = existingLanguage.StringId,
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Meta.ShouldHaveCount(3);

            responseDocument.Meta.ShouldContainKey("license").With(value =>
            {
                JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be("MIT");
            });

            responseDocument.Meta.ShouldContainKey("projectUrl").With(value =>
            {
                JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be("https://github.com/json-api-dotnet/JsonApiDotNetCore/");
            });

            responseDocument.Meta.ShouldContainKey("versions").With(value =>
            {
                JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                string?[] versionArray = element.EnumerateArray().Select(arrayItem => arrayItem.GetString()).ToArray();

                versionArray.ShouldHaveCount(4);
                versionArray.Should().Contain("v4.0.0");
                versionArray.Should().Contain("v3.1.0");
                versionArray.Should().Contain("v2.5.2");
                versionArray.Should().Contain("v1.3.1");
            });
        }
    }
}
