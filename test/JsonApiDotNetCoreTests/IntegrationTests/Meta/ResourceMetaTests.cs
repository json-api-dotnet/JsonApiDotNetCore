using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

public sealed class ResourceMetaTests : IClassFixture<IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> _testContext;
    private readonly MetaFakers _fakers = new();

    public ResourceMetaTests(IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ProductFamiliesController>();
        testContext.UseController<SupportTicketsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceDefinition<SupportTicketDefinition>();

            services.AddSingleton<ResourceDefinitionHitCounter>();
        });

        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();
        hitCounter.Reset();
    }

    [Fact]
    public async Task Returns_resource_meta_from_ResourceDefinition()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<SupportTicket> tickets = _fakers.SupportTicket.GenerateList(3);
        tickets[0].Description = $"Critical: {tickets[0].Description}";
        tickets[2].Description = $"Critical: {tickets[2].Description}";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<SupportTicket>();
            dbContext.SupportTickets.AddRange(tickets);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Meta.Should().ContainKey("hasHighPriority");
        responseDocument.Data.ManyValue[1].Meta.Should().BeNull();
        responseDocument.Data.ManyValue[2].Meta.Should().ContainKey("hasHighPriority");

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(SupportTicket), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(SupportTicket), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(SupportTicket), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Returns_resource_meta_from_ResourceDefinition_in_included_resources()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        ProductFamily family = _fakers.ProductFamily.GenerateOne();
        family.Tickets = _fakers.SupportTicket.GenerateList(1);
        family.Tickets[0].Description = $"Critical: {family.Tickets[0].Description}";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<ProductFamily>();
            dbContext.ProductFamilies.Add(family);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/productFamilies/{family.StringId}?include=tickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Meta.Should().ContainKey("hasHighPriority");

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(SupportTicket), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }
}
