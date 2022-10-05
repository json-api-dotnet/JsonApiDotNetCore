using System.Net;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Experiments;

public sealed class ExperimentsTests : IClassFixture<IntegrationTestContext<TestableStartup<ExperimentsDbContext>, ExperimentsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ExperimentsDbContext>, ExperimentsDbContext> _testContext;
    private readonly ExperimentsFakers _fakers = new();

    public ExperimentsTests(IntegrationTestContext<TestableStartup<ExperimentsDbContext>, ExperimentsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CustomersController>();
        testContext.UseController<OrdersController>();
        testContext.UseController<ShoppingBasketsController>();

        testContext.PostConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Singleton<TimeProvider>(new FrozenTimeProvider(1.January(2005).ToDateTimeOffset())));

            services.AddResourceService<SoftDeletionAwareResourceService<Company, long>>();
            services.AddResourceService<SoftDeletionAwareResourceService<Department, long>>();
        });
    }

    [Fact]
    public async Task Can_delete_resource()
    {
        // Arrange
        Order existingOrder = _fakers.Order.Generate();
        existingOrder.Customer = _fakers.Customer.Generate();
        existingOrder.Parent = _fakers.Order.Generate();
        existingOrder.Parent.Customer = existingOrder.Customer;

        List<ShoppingBasket> existingBaskets = _fakers.ShoppingBasket.Generate(3);
        existingBaskets[0].CurrentOrder = existingOrder;
        existingBaskets[1].CurrentOrder = existingOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Orders.Add(existingOrder);
            await dbContext.SaveChangesAsync();

            existingOrder.Customer.FirstOrder = existingOrder;
            existingOrder.Customer.LastOrder = existingOrder;
            dbContext.ShoppingBaskets.AddRange(existingBaskets);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrder.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();
    }
}
