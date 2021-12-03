using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Mixed
{
    public sealed class AtomicSerializationTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new();

        public AtomicSerializationTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OperationsController>();

            // These routes need to be registered in ASP.NET for rendering links to resource/relationship endpoints.
            testContext.UseController<TextLanguagesController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<ImplicitlyChangingTextLanguageDefinition>();

                services.AddSingleton<ResourceDefinitionHitCounter>();
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeExceptionStackTraceInErrors = false;
            options.IncludeJsonApiVersion = true;
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Hides_data_for_void_operation()
        {
            // Arrange
            Performer existingPerformer = _fakers.Performer.Generate();

            TextLanguage newLanguage = _fakers.TextLanguage.Generate();
            newLanguage.Id = Guid.NewGuid();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Performers.Add(existingPerformer);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "performers",
                            id = existingPerformer.StringId,
                            attributes = new
                            {
                            }
                        }
                    },
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
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""jsonapi"": {
    ""version"": ""1.1"",
    ""ext"": [
      ""https://jsonapi.org/ext/atomic""
    ]
  },
  ""links"": {
    ""self"": ""http://localhost/operations""
  },
  ""atomic:results"": [
    {
      ""data"": null
    },
    {
      ""data"": {
        ""type"": ""textLanguages"",
        ""id"": """ + newLanguage.StringId + @""",
        ""attributes"": {
          ""isoCode"": """ + newLanguage.IsoCode + @" (changed)""
        },
        ""relationships"": {
          ""lyrics"": {
            ""links"": {
              ""self"": ""http://localhost/textLanguages/" + newLanguage.StringId + @"/relationships/lyrics"",
              ""related"": ""http://localhost/textLanguages/" + newLanguage.StringId + @"/lyrics""
            }
          }
        },
        ""links"": {
          ""self"": ""http://localhost/textLanguages/" + newLanguage.StringId + @"""
        }
      }
    }
  ]
}");
        }

        [Fact]
        public async Task Includes_version_with_ext_on_error_in_operations_endpoint()
        {
            // Arrange
            string musicTrackId = Unknown.StringId.For<MusicTrack, Guid>();

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = musicTrackId
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            string errorId = JsonApiStringConverter.ExtractErrorId(responseDocument);

            responseDocument.Should().BeJson(@"{
  ""jsonapi"": {
    ""version"": ""1.1"",
    ""ext"": [
      ""https://jsonapi.org/ext/atomic""
    ]
  },
  ""links"": {
    ""self"": ""http://localhost/operations""
  },
  ""errors"": [
    {
      ""id"": """ + errorId + @""",
      ""status"": ""404"",
      ""title"": ""The requested resource does not exist."",
      ""detail"": ""Resource of type 'musicTracks' with ID '" + musicTrackId + @"' does not exist."",
      ""source"": {
        ""pointer"": ""/atomic:operations[0]""
      }
    }
  ]
}");
        }
    }
}
