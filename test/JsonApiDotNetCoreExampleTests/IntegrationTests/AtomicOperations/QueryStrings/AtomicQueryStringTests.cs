using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.QueryStrings
{
    public sealed class AtomicQueryStringTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private static readonly DateTime _frozenTime = 30.July(2018).At(13, 46, 12);

        private readonly ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicQueryStringTests(ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddControllersFromExampleProject();

                services.AddSingleton<ISystemClock>(new FrozenSystemClock {UtcNow = _frozenTime});
                services.AddScoped<IResourceDefinition<MusicTrack, Guid>, MusicTrackReleaseDefinition>();
            });

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowQueryStringOverrideForSerializerDefaultValueHandling = true;
            options.AllowQueryStringOverrideForSerializerNullValueHandling = true;
        }

        [Fact]
        public async Task Cannot_include_on_operations_endpoint()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            var route = "/operations?include=recordCompanies";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'include' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("include");
        }

        [Fact]
        public async Task Cannot_filter_on_operations_endpoint()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            var route = "/operations?filter=equals(id,'1')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'filter' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter");
        }

        [Fact]
        public async Task Cannot_sort_on_operations_endpoint()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            var route = "/operations?sort=-id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'sort' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort");
        }

        [Fact]
        public async Task Cannot_use_pagination_number_on_operations_endpoint()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            var route = "/operations?page[number]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'page[number]' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[number]");
        }

        [Fact]
        public async Task Cannot_use_pagination_size_on_operations_endpoint()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            var route = "/operations?page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'page[size]' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[size]");
        }

        [Fact]
        public async Task Cannot_use_sparse_fieldset_on_operations_endpoint()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            var route = "/operations?fields[recordCompanies]=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'fields[recordCompanies]' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("fields[recordCompanies]");
        }

        [Fact]
        public async Task Can_use_Queryable_handler_on_resource_endpoint()
        {
            // Arrange
            var musicTracks = _fakers.MusicTrack.Generate(3);
            musicTracks[0].ReleasedAt = _frozenTime.AddMonths(5);
            musicTracks[1].ReleasedAt = _frozenTime.AddMonths(-5);
            musicTracks[2].ReleasedAt = _frozenTime.AddMonths(-1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<MusicTrack>();
                dbContext.MusicTracks.AddRange(musicTracks);

                await dbContext.SaveChangesAsync();
            });

            var route = "/musicTracks?isRecentlyReleased=true";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(musicTracks[2].StringId);
        }

        [Fact]
        public async Task Cannot_use_Queryable_handler_on_operations_endpoint()
        {
            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    }
                }
            };

            var route = "/operations?isRecentlyReleased=true";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Unknown query string parameter.");
            responseDocument.Errors[0].Detail.Should().Be("Query string parameter 'isRecentlyReleased' is unknown. Set 'AllowUnknownQueryStringParameters' to 'true' in options to ignore unknown parameters.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("isRecentlyReleased");
        }

        [Fact]
        public async Task Can_use_defaults_on_operations_endpoint()
        {
            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newTrackLength = _fakers.MusicTrack.Generate().LengthInSeconds;

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            attributes = new
                            {
                                title = newTrackTitle,
                                lengthInSeconds = newTrackLength
                            }
                        }
                    }
                }
            };

            var route = "/operations?defaults=false";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Attributes.Should().HaveCount(2);
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);
            responseDocument.Results[0].SingleData.Attributes["lengthInSeconds"].As<decimal?>().Should().BeApproximately(newTrackLength, 0.00000000001M);
        }

        [Fact]
        public async Task Can_use_nulls_on_operations_endpoint()
        {
            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newTrackLength = _fakers.MusicTrack.Generate().LengthInSeconds;

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            attributes = new
                            {
                                title = newTrackTitle,
                                lengthInSeconds = newTrackLength
                            }
                        }
                    }
                }
            };

            var route = "/operations?nulls=false";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Attributes.Should().HaveCount(2);
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);
            responseDocument.Results[0].SingleData.Attributes["lengthInSeconds"].As<decimal?>().Should().BeApproximately(newTrackLength, 0.00000000001M);
        }
    }
}
