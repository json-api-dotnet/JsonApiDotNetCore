using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Meta
{
    public sealed class AtomicResourceMetaTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new();

        public AtomicResourceMetaTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OperationsController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<MusicTrackMetaDefinition>();
                services.AddResourceDefinition<TextLanguageMetaDefinition>();

                services.AddSingleton<ResourceDefinitionHitCounter>();
            });

            var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();
            hitCounter.Reset();
        }

        [Fact]
        public async Task Returns_resource_meta_in_create_resource_with_side_effects()
        {
            // Arrange
            var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

            string newTitle1 = _fakers.MusicTrack.Generate().Title;
            string newTitle2 = _fakers.MusicTrack.Generate().Title;

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            attributes = new
                            {
                                title = newTitle1,
                                releasedAt = 1.January(2018)
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            attributes = new
                            {
                                title = newTitle2,
                                releasedAt = 23.August(1994)
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

            responseDocument.Results.ShouldHaveCount(2);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Meta.ShouldHaveCount(1);

                resource.Meta.ShouldContainKey("copyright").With(value =>
                {
                    JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                    element.GetString().Should().Be("(C) 2018. All rights reserved.");
                });
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Meta.ShouldHaveCount(1);

                resource.Meta.ShouldContainKey("copyright").With(value =>
                {
                    JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                    element.GetString().Should().Be("(C) 1994. All rights reserved.");
                });
            });

            hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
            {
                (typeof(MusicTrack), ResourceDefinitionExtensibilityPoints.GetMeta),
                (typeof(MusicTrack), ResourceDefinitionExtensibilityPoints.GetMeta)
            }, options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task Returns_resource_meta_in_update_resource_with_side_effects()
        {
            // Arrange
            var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

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

            responseDocument.Results.ShouldHaveCount(1);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Meta.ShouldHaveCount(1);

                resource.Meta.ShouldContainKey("notice").With(value =>
                {
                    JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                    element.GetString().Should().Be(TextLanguageMetaDefinition.NoticeText);
                });
            });

            hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
            {
                (typeof(TextLanguage), ResourceDefinitionExtensibilityPoints.GetMeta)
            }, options => options.WithStrictOrdering());
        }
    }
}
