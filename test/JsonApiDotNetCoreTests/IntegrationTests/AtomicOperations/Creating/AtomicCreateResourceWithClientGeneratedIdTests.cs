using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Creating
{
    public sealed class AtomicCreateResourceWithClientGeneratedIdTests
        : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new();

        public AtomicCreateResourceWithClientGeneratedIdTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OperationsController>();

            // These routes need to be registered in ASP.NET for rendering links to resource/relationship endpoints.
            testContext.UseController<TextLanguagesController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<ImplicitlyChangingTextLanguageDefinition>();
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects()
        {
            // Arrange
            TextLanguage newLanguage = _fakers.TextLanguage.Generate();
            newLanguage.Id = Guid.NewGuid();

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "textLanguages",
                            id = newLanguage.StringId,
                            attributes = new
                            {
                                isoCode = newLanguage.IsoCode
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

            string isoCode = $"{newLanguage.IsoCode}{ImplicitlyChangingTextLanguageDefinition.Suffix}";

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].Data.SingleValue.Should().NotBeNull();
            responseDocument.Results[0].Data.SingleValue.Type.Should().Be("textLanguages");
            responseDocument.Results[0].Data.SingleValue.Attributes["isoCode"].Should().Be(isoCode);
            responseDocument.Results[0].Data.SingleValue.Attributes.Should().NotContainKey("isRightToLeft");
            responseDocument.Results[0].Data.SingleValue.Relationships.Should().NotBeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                TextLanguage languageInDatabase = await dbContext.TextLanguages.FirstWithIdAsync(newLanguage.Id);

                languageInDatabase.IsoCode.Should().Be(isoCode);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects()
        {
            // Arrange
            MusicTrack newTrack = _fakers.MusicTrack.Generate();
            newTrack.Id = Guid.NewGuid();

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
                            id = newTrack.StringId,
                            attributes = new
                            {
                                title = newTrack.Title,
                                lengthInSeconds = newTrack.LengthInSeconds,
                                releasedAt = newTrack.ReleasedAt
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.FirstWithIdAsync(newTrack.Id);

                trackInDatabase.Title.Should().Be(newTrack.Title);
                trackInDatabase.LengthInSeconds.Should().BeApproximately(newTrack.LengthInSeconds);
            });
        }

        [Fact]
        public async Task Cannot_create_resource_for_existing_client_generated_ID()
        {
            // Arrange
            TextLanguage existingLanguage = _fakers.TextLanguage.Generate();
            existingLanguage.Id = Guid.NewGuid();

            TextLanguage languageToCreate = _fakers.TextLanguage.Generate();
            languageToCreate.Id = existingLanguage.Id;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TextLanguages.Add(languageToCreate);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "textLanguages",
                            id = languageToCreate.StringId,
                            attributes = new
                            {
                                isoCode = languageToCreate.IsoCode
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Conflict);
            error.Title.Should().Be("Another resource with the specified ID already exists.");
            error.Detail.Should().Be($"Another resource of type 'textLanguages' with ID '{languageToCreate.StringId}' already exists.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_create_resource_for_incompatible_ID()
        {
            // Arrange
            string guid = Unknown.StringId.Guid;

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
                            id = guid,
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Incompatible 'id' value found.");
            error.Detail.Should().Be($"Failed to convert '{guid}' of type 'String' to type 'Int32'.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data/id");
        }

        [Fact]
        public async Task Cannot_create_resource_for_ID_and_local_ID()
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
                            type = "lyrics",
                            id = Unknown.StringId.For<Lyric, long>(),
                            lid = "local-1"
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: The 'id' and 'lid' element are mutually exclusive.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        }
    }
}
