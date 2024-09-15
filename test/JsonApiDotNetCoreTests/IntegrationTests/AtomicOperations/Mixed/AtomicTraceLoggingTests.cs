using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Mixed;

public sealed class AtomicTraceLoggingTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicTraceLoggingTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        testContext.ConfigureLogging(options =>
        {
            var loggerProvider = new CapturingLoggerProvider((category, level) =>
                level >= LogLevel.Trace && category.StartsWith("JsonApiDotNetCore.", StringComparison.Ordinal));

            options.AddProvider(loggerProvider);
            options.SetMinimumLevel(LogLevel.Trace);

            options.Services.AddSingleton(loggerProvider);
        });
    }

    [Fact]
    public async Task Logs_execution_flow_at_Trace_level_on_operations_request()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();
        existingTrack.Lyric = _fakers.Lyric.GenerateOne();
        existingTrack.OwnedBy = _fakers.RecordCompany.GenerateOne();
        existingTrack.Performers = _fakers.Performer.GenerateList(1);

        string newGenre = _fakers.MusicTrack.GenerateOne().Genre!;

        Lyric existingLyric = _fakers.Lyric.GenerateOne();
        RecordCompany existingCompany = _fakers.RecordCompany.GenerateOne();
        Performer existingPerformer = _fakers.Performer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingTrack, existingLyric, existingCompany, existingPerformer);
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
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        attributes = new
                        {
                            genre = newGenre
                        },
                        relationships = new
                        {
                            lyric = new
                            {
                                data = new
                                {
                                    type = "lyrics",
                                    id = existingLyric.StringId
                                }
                            },
                            ownedBy = new
                            {
                                data = new
                                {
                                    type = "recordCompanies",
                                    id = existingCompany.StringId
                                }
                            },
                            performers = new
                            {
                                data = new[]
                                {
                                    new
                                    {
                                        type = "performers",
                                        id = existingPerformer.StringId
                                    }
                                }
                            }
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

        IReadOnlyList<string> logLines = loggerProvider.GetLines();

        logLines.Should().BeEquivalentTo(new[]
        {
            $$"""
            [TRACE] Received POST request at 'http://localhost/operations' with body: <<{
              "atomic:operations": [
                {
                  "op": "update",
                  "data": {
                    "type": "musicTracks",
                    "id": "{{existingTrack.StringId}}",
                    "attributes": {
                      "genre": "{{newGenre}}"
                    },
                    "relationships": {
                      "lyric": {
                        "data": {
                          "type": "lyrics",
                          "id": "{{existingLyric.StringId}}"
                        }
                      },
                      "ownedBy": {
                        "data": {
                          "type": "recordCompanies",
                          "id": "{{existingCompany.StringId}}"
                        }
                      },
                      "performers": {
                        "data": [
                          {
                            "type": "performers",
                            "id": "{{existingPerformer.StringId}}"
                          }
                        ]
                      }
                    }
                  }
                }
              ]
            }>>
            """,
            $$"""
            [TRACE] Entering PostOperationsAsync(operations: [
              {
                "Resource": {
                  "Id": "{{existingTrack.StringId}}",
                  "Genre": "{{newGenre}}",
                  "ReleasedAt": "0001-01-01T00:00:00+00:00",
                  "Lyric": {
                    "CreatedAt": "0001-01-01T00:00:00+00:00",
                    "Id": {{existingLyric.Id}},
                    "StringId": "{{existingLyric.StringId}}"
                  },
                  "OwnedBy": {
                    "Tracks": [],
                    "Id": {{existingCompany.Id}},
                    "StringId": "{{existingCompany.StringId}}"
                  },
                  "Performers": [
                    {
                      "BornAt": "0001-01-01T00:00:00+00:00",
                      "Id": {{existingPerformer.Id}},
                      "StringId": "{{existingPerformer.StringId}}"
                    }
                  ],
                  "OccursIn": [],
                  "StringId": "{{existingTrack.StringId}}"
                },
                "TargetedFields": {
                  "Attributes": [
                    "genre"
                  ],
                  "Relationships": [
                    "lyric",
                    "ownedBy",
                    "performers"
                  ]
                },
                "Request": {
                  "Kind": "AtomicOperations",
                  "PrimaryId": "{{existingTrack.StringId}}",
                  "PrimaryResourceType": "musicTracks",
                  "IsCollection": false,
                  "IsReadOnly": false,
                  "WriteOperation": "UpdateResource"
                }
              }
            ])
            """,
            $$"""
            [TRACE] Entering UpdateAsync(id: {{existingTrack.StringId}}, resource: {
              "Id": "{{existingTrack.StringId}}",
              "Genre": "{{newGenre}}",
              "ReleasedAt": "0001-01-01T00:00:00+00:00",
              "Lyric": {
                "CreatedAt": "0001-01-01T00:00:00+00:00",
                "Id": {{existingLyric.Id}},
                "StringId": "{{existingLyric.StringId}}"
              },
              "OwnedBy": {
                "Tracks": [],
                "Id": {{existingCompany.Id}},
                "StringId": "{{existingCompany.StringId}}"
              },
              "Performers": [
                {
                  "BornAt": "0001-01-01T00:00:00+00:00",
                  "Id": {{existingPerformer.Id}},
                  "StringId": "{{existingPerformer.StringId}}"
                }
              ],
              "OccursIn": [],
              "StringId": "{{existingTrack.StringId}}"
            })
            """,
            $$"""
            [TRACE] Entering GetForUpdateAsync(queryLayer: QueryLayer<MusicTrack>
            {
              Include: lyric,ownedBy,performers
              Filter: equals(id,'{{existingTrack.StringId}}')
            }
            )
            """,
            $$"""
            [TRACE] Entering GetAsync(queryLayer: QueryLayer<MusicTrack>
            {
              Include: lyric,ownedBy,performers
              Filter: equals(id,'{{existingTrack.StringId}}')
            }
            )
            """,
            $$"""
            [TRACE] Entering ApplyQueryLayer(queryLayer: QueryLayer<MusicTrack>
            {
              Include: lyric,ownedBy,performers
              Filter: equals(id,'{{existingTrack.StringId}}')
            }
            )
            """,
            $$"""
            [TRACE] Entering UpdateAsync(resourceFromRequest: {
              "Id": "{{existingTrack.StringId}}",
              "Genre": "{{newGenre}}",
              "ReleasedAt": "0001-01-01T00:00:00+00:00",
              "Lyric": {
                "CreatedAt": "0001-01-01T00:00:00+00:00",
                "Id": {{existingLyric.Id}},
                "StringId": "{{existingLyric.StringId}}"
              },
              "OwnedBy": {
                "Tracks": [],
                "Id": {{existingCompany.Id}},
                "StringId": "{{existingCompany.StringId}}"
              },
              "Performers": [
                {
                  "BornAt": "0001-01-01T00:00:00+00:00",
                  "Id": {{existingPerformer.Id}},
                  "StringId": "{{existingPerformer.StringId}}"
                }
              ],
              "OccursIn": [],
              "StringId": "{{existingTrack.StringId}}"
            }, resourceFromDatabase: {
              "Id": "{{existingTrack.StringId}}",
              "Title": "{{existingTrack.Title}}",
              "LengthInSeconds": {{JsonSerializer.Serialize(existingTrack.LengthInSeconds)}},
              "Genre": "{{existingTrack.Genre}}",
              "ReleasedAt": {{JsonSerializer.Serialize(existingTrack.ReleasedAt)}},
              "Lyric": {
                "Format": "{{existingTrack.Lyric.Format}}",
                "Text": {{JsonSerializer.Serialize(existingTrack.Lyric.Text)}},
                "CreatedAt": "0001-01-01T00:00:00+00:00",
                "Id": {{existingTrack.Lyric.Id}},
                "StringId": "{{existingTrack.Lyric.StringId}}"
              },
              "OwnedBy": {
                "Name": "{{existingTrack.OwnedBy.Name}}",
                "CountryOfResidence": "{{existingTrack.OwnedBy.CountryOfResidence}}",
                "Tracks": [
                  null
                ],
                "Id": {{existingTrack.OwnedBy.Id}},
                "StringId": "{{existingTrack.OwnedBy.StringId}}"
              },
              "Performers": [
                {
                  "ArtistName": "{{existingTrack.Performers[0].ArtistName}}",
                  "BornAt": {{JsonSerializer.Serialize(existingTrack.Performers[0].BornAt)}},
                  "Id": {{existingTrack.Performers[0].Id}},
                  "StringId": "{{existingTrack.Performers[0].StringId}}"
                }
              ],
              "OccursIn": [],
              "StringId": "{{existingTrack.StringId}}"
            })
            """,
            $$"""
            [TRACE] Entering GetAsync(queryLayer: QueryLayer<MusicTrack>
            {
              Filter: equals(id,'{{existingTrack.StringId}}')
            }
            )
            """,
            $$"""
            [TRACE] Entering ApplyQueryLayer(queryLayer: QueryLayer<MusicTrack>
            {
              Filter: equals(id,'{{existingTrack.StringId}}')
            }
            )
            """
        }, options => options.Using(IgnoreLineEndingsComparer.Instance).WithStrictOrdering());
    }
}
