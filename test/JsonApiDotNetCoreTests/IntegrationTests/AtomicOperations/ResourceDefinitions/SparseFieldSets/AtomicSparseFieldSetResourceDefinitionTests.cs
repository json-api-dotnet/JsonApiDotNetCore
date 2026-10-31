using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.SparseFieldSets;

public sealed class AtomicSparseFieldSetResourceDefinitionTests
    : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicSparseFieldSetResourceDefinitionTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        // These routes need to be registered in ASP.NET for rendering links to resource/relationship endpoints.
        testContext.UseController<PlaylistsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceDefinition<LyricTextDefinition>();
            services.AddResourceDefinition<PlaylistFieldSetDefinition>();

            services.AddSingleton<PlaylistFieldSetProvider>();
            services.AddSingleton<LyricPermissionProvider>();
            services.AddSingleton<ResourceDefinitionHitCounter>();
            services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
        });

        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();
        hitCounter.Reset();
    }

    [Fact]
    public async Task Resets_fieldset_evaluator_between_operations()
    {
        // Arrange
        var provider = _testContext.App.Services.GetRequiredService<PlaylistFieldSetProvider>();
        provider.IsEnabled = true;
        provider.CallCount = 0;

        string newName = _fakers.Playlist.GenerateOne().Name;

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "playlists",
                        attributes = new
                        {
                            name = newName,
                            credits = new[]
                            {
                                new
                                {
                                    role = "Producer",
                                    personName = "Alice"
                                }
                            },
                            recording = new
                            {
                                studio = "Abbey Road"
                            }
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "playlists",
                        attributes = new
                        {
                            name = newName,
                            credits = new[]
                            {
                                new
                                {
                                    role = "Producer",
                                    personName = "Alice"
                                }
                            },
                            recording = new
                            {
                                studio = "Abbey Road"
                            }
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

        responseDocument.Results.Should().HaveCount(2);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Attributes.Should().ContainKey("name");
            resource.Attributes.Should().ContainKey("recording");
            resource.Attributes.Should().NotContainKey("credits");
        });

        responseDocument.Results[1].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Attributes.Should().ContainKey("name");
            resource.Attributes.Should().ContainKey("credits");
            resource.Attributes.Should().NotContainKey("recording");
        });

        provider.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task Hides_text_in_create_resource_with_side_effects()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var provider = _testContext.App.Services.GetRequiredService<LyricPermissionProvider>();
        provider.CanViewText = false;

        List<Lyric> newLyrics = _fakers.Lyric.GenerateList(2);

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
                        attributes = new
                        {
                            format = newLyrics[0].Format,
                            text = newLyrics[0].Text
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "lyrics",
                        attributes = new
                        {
                            format = newLyrics[1].Format,
                            text = newLyrics[1].Text
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

        responseDocument.Results.Should().HaveCount(2);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Attributes.Should().ContainKey("format").WhoseValue.Should().Be(newLyrics[0].Format);
            resource.Attributes.Should().NotContainKey("text");
        });

        responseDocument.Results[1].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Attributes.Should().ContainKey("format").WhoseValue.Should().Be(newLyrics[1].Format);
            resource.Attributes.Should().NotContainKey("text");
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Lyric), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Lyric), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Hides_text_in_update_resource_with_side_effects()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var provider = _testContext.App.Services.GetRequiredService<LyricPermissionProvider>();
        provider.CanViewText = false;

        List<Lyric> existingLyrics = _fakers.Lyric.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Lyrics.AddRange(existingLyrics);
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
                        type = "lyrics",
                        id = existingLyrics[0].StringId,
                        attributes = new
                        {
                        }
                    }
                },
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "lyrics",
                        id = existingLyrics[1].StringId,
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.Should().HaveCount(2);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Attributes.Should().ContainKey("format").WhoseValue.Should().Be(existingLyrics[0].Format);
            resource.Attributes.Should().NotContainKey("text");
        });

        responseDocument.Results[1].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Attributes.Should().ContainKey("format").WhoseValue.Should().Be(existingLyrics[1].Format);
            resource.Attributes.Should().NotContainKey("text");
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Lyric), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Lyric), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet)
        }, options => options.WithStrictOrdering());
    }
}
