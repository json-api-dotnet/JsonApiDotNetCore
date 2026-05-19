using System.Net;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

public sealed class OverrideTotalResourceCountTests : IClassFixture<IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> _testContext;
    private readonly MetaFakers _fakers = new();

    public OverrideTotalResourceCountTests(IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SupportTicketsController>();

        testContext.ConfigureServices(services => services.AddResourceService<CountDisabledSupportTicketService>());

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = true;
    }

    [Fact]
    public async Task Does_not_render_resource_count_when_overridden_to_skip()
    {
        // Arrange
        List<SupportTicket> tickets = _fakers.SupportTicket.GenerateList(2);

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

        responseDocument.Meta.Should().NotContainTotal();
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class CountDisabledSupportTicketService(
        IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext, IJsonApiOptions options,
        ILoggerFactory loggerFactory, IJsonApiRequest request, IResourceChangeTracker<SupportTicket> resourceChangeTracker,
        IResourceDefinitionAccessor resourceDefinitionAccessor)
        : JsonApiResourceService<SupportTicket, long>(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request,
            resourceChangeTracker, resourceDefinitionAccessor)
    {
        protected override bool CanIncludeTotalResourceCount()
        {
            return false;
        }
    }
}
