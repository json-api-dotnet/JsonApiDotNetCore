using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    public sealed class IssueTests : IClassFixture<ExampleIntegrationTestContext<TestableStartup<IssueDbContext>, IssueDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<IssueDbContext>, IssueDbContext> _testContext;
        private readonly IssueFakers _fakers = new IssueFakers();

        public IssueTests(ExampleIntegrationTestContext<TestableStartup<IssueDbContext>, IssueDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<EngagementsController>();
            testContext.UseController<EngagementPartiesController>();
            testContext.UseController<DocumentTypesController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped<IResourceDefinition<EngagementParty, Guid>, EngagementPartyResourceDefinition>();
            });
        }

        [Fact]
        public async Task Can_get_primary_resource_by_ID()
        {
            // Arrange
            Engagement engagement = _fakers.Engagement.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Engagements.Add(engagement);
                await dbContext.SaveChangesAsync();
            });

            string route = "/engagements/" + engagement.StringId;

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(engagement.StringId);

            responseDocument.Included.Should().BeNull();
        }

        [Fact(Skip = "Fails in EF Core with: Invalid include path: 'FirstParties' - couldn't find navigation for: 'FirstParties'")]
        public async Task Can_get_primary_resource_by_ID_with_includes()
        {
            // Arrange
            Engagement engagement = _fakers.Engagement.Generate();
            engagement.Parties = _fakers.EngagementParty.Generate(3);
            engagement.Parties.ElementAt(0).Role = ModelConstants.FirstPartyRoleName;
            engagement.Parties.ElementAt(1).Role = ModelConstants.SecondPartyRoleName;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Engagements.Add(engagement);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/engagements/{engagement.StringId}?include=firstParties,secondParties";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(engagement.StringId);

            responseDocument.Included.Should().HaveCount(2);
            responseDocument.Included.Should().ContainSingle(resourceObject => resourceObject.Id == engagement.Parties.ElementAt(0).StringId);
            responseDocument.Included.Should().ContainSingle(resourceObject => resourceObject.Id == engagement.Parties.ElementAt(1).StringId);
        }

        [Fact]
        public async Task Can_get_secondary_resources_by_ID_without_sort()
        {
            // Arrange
            Engagement engagement = _fakers.Engagement.Generate();
            engagement.Parties = _fakers.EngagementParty.Generate(3);
            engagement.Parties.ElementAt(0).Role = ModelConstants.SecondPartyRoleName;
            engagement.Parties.ElementAt(1).Role = ModelConstants.FirstPartyRoleName;
            engagement.Parties.ElementAt(1).ShortName = "B";
            engagement.Parties.ElementAt(2).Role = ModelConstants.FirstPartyRoleName;
            engagement.Parties.ElementAt(2).ShortName = "A";

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Engagements.Add(engagement);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/engagements/{engagement.StringId}/parties";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(engagement.Parties.ElementAt(2).StringId);
            responseDocument.ManyData[1].Id.Should().Be(engagement.Parties.ElementAt(1).StringId);
            responseDocument.ManyData[2].Id.Should().Be(engagement.Parties.ElementAt(0).StringId);

            responseDocument.Included.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_secondary_resources_by_ID_with_sort()
        {
            // Arrange
            Engagement engagement = _fakers.Engagement.Generate();
            engagement.Parties = _fakers.EngagementParty.Generate(2);
            engagement.Parties.ElementAt(0).ShortName = "B";
            engagement.Parties.ElementAt(1).ShortName = "A";

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Engagements.Add(engagement);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/engagements/{engagement.StringId}/parties?sort=shortName";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(engagement.Parties.ElementAt(1).StringId);
            responseDocument.ManyData[1].Id.Should().Be(engagement.Parties.ElementAt(0).StringId);

            responseDocument.Included.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_secondary_resources_by_ID_with_descending_sort()
        {
            // Arrange
            Engagement engagement = _fakers.Engagement.Generate();
            engagement.Parties = _fakers.EngagementParty.Generate(2);
            engagement.Parties.ElementAt(0).ShortName = "A";
            engagement.Parties.ElementAt(1).ShortName = "B";

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Engagements.Add(engagement);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/engagements/{engagement.StringId}/parties?sort=-shortName";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(engagement.Parties.ElementAt(1).StringId);
            responseDocument.ManyData[1].Id.Should().Be(engagement.Parties.ElementAt(0).StringId);

            responseDocument.Included.Should().BeNull();
        }

        [Fact(Skip = "Fails in EF Core with: Invalid include path: 'FirstParties' - couldn't find navigation for: 'FirstParties'")]
        public async Task Can_get_unmapped_secondary_resources_by_ID()
        {
            // Arrange
            Engagement engagement = _fakers.Engagement.Generate();
            engagement.Parties = _fakers.EngagementParty.Generate(3);
            engagement.Parties.ElementAt(0).Role = ModelConstants.FirstPartyRoleName;
            engagement.Parties.ElementAt(1).Role = ModelConstants.FirstPartyRoleName;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Engagements.Add(engagement);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/engagements/{engagement.StringId}/firstParties";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData.Should().ContainSingle(resourceObject => resourceObject.Id == engagement.Parties.ElementAt(0).StringId);
            responseDocument.ManyData.Should().ContainSingle(resourceObject => resourceObject.Id == engagement.Parties.ElementAt(1).StringId);

            responseDocument.Included.Should().BeNull();
        }
    }
}
