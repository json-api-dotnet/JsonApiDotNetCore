using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Mixed
{
    public sealed class AtomicSerializationTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private const string JsonDateTimeOffsetFormatSpecifier = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new();

        public AtomicSerializationTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OperationsController>();

            // These routes need to be registered in ASP.NET for rendering links to resource/relationship endpoints.
            testContext.UseController<PerformersController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeExceptionStackTraceInErrors = false;
            options.IncludeJsonApiVersion = true;
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Includes_version_with_ext_on_operations_endpoint()
        {
            // Arrange
            Performer newPerformer = _fakers.Performer.Generate();
            newPerformer.Id = Unknown.TypedId.Int32;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Performer>();
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
                            type = "performers",
                            id = newPerformer.StringId,
                            attributes = new
                            {
                                artistName = newPerformer.ArtistName,
                                bornAt = newPerformer.BornAt
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
  ""atomic:results"": [
    {
      ""data"": {
        ""type"": ""performers"",
        ""id"": """ + newPerformer.StringId + @""",
        ""attributes"": {
          ""artistName"": """ + newPerformer.ArtistName + @""",
          ""bornAt"": """ + newPerformer.BornAt.ToString(JsonDateTimeOffsetFormatSpecifier) + @"""
        },
        ""links"": {
          ""self"": ""http://localhost/performers/" + newPerformer.StringId + @"""
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
