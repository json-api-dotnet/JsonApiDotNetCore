using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

public sealed class TopLevelCountTests : IClassFixture<IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> _testContext;
    private readonly MetaFakers _fakers = new();

    public TopLevelCountTests(IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ProductFamiliesController>();
        testContext.UseController<SupportTicketsController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = true;
    }

    [Fact]
    public async Task Renders_resource_count_at_primary_resources_endpoint_with_filter()
    {
        // Arrange
        List<SupportTicket> tickets = _fakers.SupportTicket.GenerateList(2);

        tickets[1].Description = "Update firmware version";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<SupportTicket>();
            dbContext.SupportTickets.AddRange(tickets);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/supportTickets?filter=startsWith(description,'Update ')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Meta.Should().ContainTotal(1);
    }

    [Fact]
    public async Task Renders_resource_count_at_secondary_resources_endpoint_with_filter()
    {
        // Arrange
        ProductFamily family = _fakers.ProductFamily.GenerateOne();
        family.Tickets = _fakers.SupportTicket.GenerateList(2);

        family.Tickets[1].Description = "Update firmware version";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(family);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/productFamilies/{family.StringId}/tickets?filter=contains(description,'firmware')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Meta.Should().ContainTotal(1);
    }

    [Fact]
    public async Task Renders_resource_count_for_empty_collection()
    {
        // Arrange
        await _testContext.RunOnDatabaseAsync(async dbContext => await dbContext.ClearTableAsync<SupportTicket>());

        const string route = "/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Meta.Should().ContainTotal(0);
    }

    [Fact]
    public async Task Hides_resource_count_in_create_resource_response()
    {
        // Arrange
        string newDescription = _fakers.SupportTicket.GenerateOne().Description;

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                attributes = new
                {
                    description = newDescription
                }
            }
        };

        const string route = "/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Meta.Should().BeNull();
    }

    [Fact]
    public async Task Hides_resource_count_in_update_resource_response()
    {
        // Arrange
        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        string newDescription = _fakers.SupportTicket.GenerateOne().Description;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.SupportTickets.Add(existingTicket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                id = existingTicket.StringId,
                attributes = new
                {
                    description = newDescription
                }
            }
        };

        string route = $"/supportTickets/{existingTicket.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Meta.Should().BeNull();
    }
}
