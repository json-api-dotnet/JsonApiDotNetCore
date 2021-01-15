using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Links
{
    public sealed class AtomicLinksTests
        : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicLinksTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                var part = new AssemblyPart(typeof(EmptyStartup).Assembly);
                services.AddMvcCore().ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));

                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });
        }

        [Fact]
        public async Task Create_resource_with_side_effects_returns_relative_links()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.Namespace = "api";
            options.UseRelativeLinks = true;

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "textLanguages",
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
                            type = "recordCompanies",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            
            var newLanguageId = Guid.Parse(responseDocument.Results[0].SingleData.Id);
            
            responseDocument.Results[0].SingleData.Links.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Links.Self.Should().Be("/api/textLanguages/" + newLanguageId);
            responseDocument.Results[0].SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Self.Should().Be($"/api/textLanguages/{newLanguageId}/relationships/lyrics");
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Related.Should().Be($"/api/textLanguages/{newLanguageId}/lyrics");

            responseDocument.Results[1].SingleData.Should().NotBeNull();

            var newCompanyId = short.Parse(responseDocument.Results[1].SingleData.Id);

            responseDocument.Results[1].SingleData.Links.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Links.Self.Should().Be("/api/recordCompanies/" + newCompanyId);
            responseDocument.Results[1].SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Self.Should().Be($"/api/recordCompanies/{newCompanyId}/relationships/tracks");
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Related.Should().Be($"/api/recordCompanies/{newCompanyId}/tracks");
        }

        [Fact]
        public async Task Update_resource_with_side_effects_returns_absolute_links()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.Namespace = null;
            options.UseRelativeLinks = false;

            var existingLanguage = _fakers.TextLanguage.Generate();
            var existingCompany = _fakers.RecordCompany.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingLanguage, existingCompany);
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
                            type = "textLanguages",
                            id = existingLanguage.StringId,
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
                            type = "recordCompanies",
                            id = existingCompany.StringId,
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Links.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Links.Self.Should().Be("http://localhost/textLanguages/" + existingLanguage.StringId);
            responseDocument.Results[0].SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Self.Should().Be($"http://localhost/textLanguages/{existingLanguage.StringId}/relationships/lyrics");
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Related.Should().Be($"http://localhost/textLanguages/{existingLanguage.StringId}/lyrics");

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Links.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Links.Self.Should().Be("http://localhost/recordCompanies/" + existingCompany.StringId);
            responseDocument.Results[1].SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Self.Should().Be($"http://localhost/recordCompanies/{existingCompany.StringId}/relationships/tracks");
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Related.Should().Be($"http://localhost/recordCompanies/{existingCompany.StringId}/tracks");
        }
    }
}
