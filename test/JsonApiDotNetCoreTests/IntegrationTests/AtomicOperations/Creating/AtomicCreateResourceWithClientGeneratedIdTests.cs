using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Creating;

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

        testContext.ConfigureServices(services =>
        {
            services.AddResourceDefinition<AssignIdToTextLanguageDefinition>();

            services.AddSingleton<ResourceDefinitionHitCounter>();
        });
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        TextLanguage newLanguage = _fakers.TextLanguage.GenerateOne();
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string isoCode = $"{newLanguage.IsoCode}{ImplicitlyChangingTextLanguageDefinition.Suffix}";

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("textLanguages");
            resource.Attributes.ShouldContainKey("isoCode").With(value => value.Should().Be(isoCode));
            resource.Attributes.Should().NotContainKey("isRightToLeft");
            resource.Relationships.ShouldNotBeEmpty();
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TextLanguage languageInDatabase = await dbContext.TextLanguages.FirstWithIdAsync(newLanguage.Id);

            languageInDatabase.IsoCode.Should().Be(isoCode);
        });
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Can_create_resource_with_client_generated_guid_ID_having_no_side_effects(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        MusicTrack newTrack = _fakers.MusicTrack.GenerateOne();
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.FirstWithIdAsync(newTrack.Id);

            trackInDatabase.Title.Should().Be(newTrack.Title);
            trackInDatabase.LengthInSeconds.Should().BeApproximately(newTrack.LengthInSeconds);
        });
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    public async Task Can_create_resource_for_missing_client_generated_ID_having_side_effects(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        string? newIsoCode = _fakers.TextLanguage.GenerateOne().IsoCode;

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
                        attributes = new
                        {
                            isoCode = newIsoCode
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string isoCode = $"{newIsoCode}{ImplicitlyChangingTextLanguageDefinition.Suffix}";

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("textLanguages");
            resource.Attributes.ShouldContainKey("isoCode").With(value => value.Should().Be(isoCode));
            resource.Relationships.ShouldNotBeEmpty();
        });

        Guid newLanguageId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TextLanguage languageInDatabase = await dbContext.TextLanguages.FirstWithIdAsync(newLanguageId);

            languageInDatabase.IsoCode.Should().Be(isoCode);
        });
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_for_missing_client_generated_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        string? newIsoCode = _fakers.TextLanguage.GenerateOne().IsoCode;

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
                        attributes = new
                        {
                            isoCode = newIsoCode
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_for_existing_client_generated_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        TextLanguage existingLanguage = _fakers.TextLanguage.GenerateOne();
        existingLanguage.Id = Guid.NewGuid();

        TextLanguage languageToCreate = _fakers.TextLanguage.GenerateOne();
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'textLanguages' with ID '{languageToCreate.StringId}' already exists.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_for_incompatible_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible 'id' value found.");
        error.Detail.Should().Be($"Failed to convert '{guid}' of type 'String' to type 'Int32'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/id");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    public async Task Can_create_resource_with_local_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        string newTitle = _fakers.MusicTrack.GenerateOne().Title;

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
                        lid = "new-server-id",
                        attributes = new
                        {
                            title = newTitle
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("musicTracks");
            resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTitle));
            resource.Relationships.Should().BeNull();
        });

        Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack languageInDatabase = await dbContext.MusicTracks.FirstWithIdAsync(newTrackId);

            languageInDatabase.Title.Should().Be(newTitle);
        });
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_with_local_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

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
                        lid = "new-server-id"
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'lid' element cannot be used because a client-generated ID is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/lid");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_for_ID_and_local_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' and 'lid' element are mutually exclusive.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }
}
