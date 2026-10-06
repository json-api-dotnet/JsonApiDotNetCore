using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete;

public sealed class ClientSetNullOnDeleteTests : IClassFixture<IntegrationTestContext<ClientSetNullOnDeleteStartup, ClientSetNullOnDeleteDbContext>>
{
    private readonly IntegrationTestContext<ClientSetNullOnDeleteStartup, ClientSetNullOnDeleteDbContext> _testContext;
    private readonly ClientSetNullOnDeleteFakers _fakers = new();

    public ClientSetNullOnDeleteTests(IntegrationTestContext<ClientSetNullOnDeleteStartup, ClientSetNullOnDeleteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OrdersController>();
        testContext.UseController<ShoppingBasketsController>();
        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task Can_delete_resource_with_multiple_incoming_optional_relationships_from_different_entities()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        ShoppingBasket existingBasket = _fakers.ShoppingBasket.GenerateOne();
        existingBasket.CurrentOrder = existingOrder;

        List<SupportTicket> existingTickets = _fakers.SupportTicket.GenerateList(2);
        existingTickets[0].Order = existingOrder;
        existingTickets[1].Order = existingOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.SupportTickets.AddRange(existingTickets);
            dbContext.ShoppingBaskets.Add(existingBasket);
            await dbContext.SaveChangesAsync();

            existingCustomer.FirstOrder = existingOrder;
            existingCustomer.LastOrder = existingOrder;
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrder.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Customer customerInDatabase = await dbContext.Customers
                .Include(customer => customer.FirstOrder)
                .Include(customer => customer.LastOrder)
                .FirstWithIdAsync(existingCustomer.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            customerInDatabase.FirstOrder.Should().BeNull();
            customerInDatabase.LastOrder.Should().BeNull();

            ShoppingBasket basketInDatabase = await dbContext.ShoppingBaskets.Include(basket => basket.CurrentOrder).FirstWithIdAsync(existingBasket.Id);

            basketInDatabase.CurrentOrder.Should().BeNull();

            List<long> ticketIds =
            [
                existingTickets[0].Id,
                existingTickets[1].Id
            ];

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            List<SupportTicket> ticketsInDatabase = await dbContext.SupportTickets
                .Include(ticket => ticket.Order)
                .Where(ticket => ticketIds.Contains(ticket.Id))
                .ToListAsync();

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            ticketsInDatabase.Should().HaveCount(2);
            ticketsInDatabase.Should().OnlyContain(ticket => ticket.Order == null);
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_multiple_incoming_optional_OneToOne_relationships_from_different_entities()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        ShoppingBasket existingBasket = _fakers.ShoppingBasket.GenerateOne();
        existingBasket.CurrentOrder = existingOrder;

        OrderAudit existingAudit = _fakers.OrderAudit.GenerateOne();
        existingAudit.Order = existingOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingBasket, existingAudit);
            await dbContext.SaveChangesAsync();

            existingCustomer.FirstOrder = existingOrder;
            existingCustomer.LastOrder = existingOrder;
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrder.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Customer customerInDatabase = await dbContext.Customers
                .Include(customer => customer.FirstOrder)
                .Include(customer => customer.LastOrder)
                .FirstWithIdAsync(existingCustomer.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            customerInDatabase.FirstOrder.Should().BeNull();
            customerInDatabase.LastOrder.Should().BeNull();

            ShoppingBasket basketInDatabase = await dbContext.ShoppingBaskets.Include(basket => basket.CurrentOrder).FirstWithIdAsync(existingBasket.Id);

            basketInDatabase.CurrentOrder.Should().BeNull();

            OrderAudit auditInDatabase = await dbContext.OrderAudits.Include(audit => audit.Order).FirstAsync(audit => audit.Id == existingAudit.Id);

            auditInDatabase.Order.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_intermediate_resource_in_self_referencing_hierarchy()
    {
        // Arrange
        Customer existingCustomer = _fakers.Customer.GenerateOne();

        Order existingGrandparentOrder = _fakers.Order.GenerateOne();
        existingGrandparentOrder.Customer = existingCustomer;

        Order existingParentOrder = _fakers.Order.GenerateOne();
        existingParentOrder.Customer = existingCustomer;
        existingParentOrder.Parent = existingGrandparentOrder;

        Order existingChildOrder = _fakers.Order.GenerateOne();
        existingChildOrder.Customer = existingCustomer;
        existingChildOrder.Parent = existingParentOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Orders.Add(existingChildOrder);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingParentOrder.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order grandparentOrderInDatabase = await dbContext.Orders.Include(order => order.Parent).FirstWithIdAsync(existingGrandparentOrder.Id);

            grandparentOrderInDatabase.Parent.Should().BeNull();

            Order? parentOrderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingParentOrder.Id);

            parentOrderInDatabase.Should().BeNull();

            Order childOrderInDatabase = await dbContext.Orders.Include(order => order.Parent).FirstWithIdAsync(existingChildOrder.Id);

            childOrderInDatabase.Parent.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_self_referencing_OneToMany_relationship()
    {
        // Arrange
        Order existingParentOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingParentOrder.Customer = existingCustomer;

        Order existingChildOrder1 = _fakers.Order.GenerateOne();
        existingChildOrder1.Customer = existingCustomer;
        existingChildOrder1.Parent = existingParentOrder;

        Order existingChildOrder2 = _fakers.Order.GenerateOne();
        existingChildOrder2.Customer = existingCustomer;
        existingChildOrder2.Parent = existingParentOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Orders.AddRange(existingChildOrder1, existingChildOrder2);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingParentOrder.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? parentOrderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingParentOrder.Id);

            parentOrderInDatabase.Should().BeNull();

            Order childOrderInDatabase1 = await dbContext.Orders.Include(order => order.Parent).FirstWithIdAsync(existingChildOrder1.Id);
            Order childOrderInDatabase2 = await dbContext.Orders.Include(order => order.Parent).FirstWithIdAsync(existingChildOrder2.Id);

            childOrderInDatabase1.Parent.Should().BeNull();
            childOrderInDatabase2.Parent.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_circular_self_reference()
    {
        // Arrange
        Order existingOrderA = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrderA.Customer = existingCustomer;

        Order existingOrderB = _fakers.Order.GenerateOne();
        existingOrderB.Customer = existingCustomer;
        existingOrderA.Parent = existingOrderB;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Orders.Add(existingOrderA);
            await dbContext.SaveChangesAsync();

            existingOrderB.Parent = existingOrderA;
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrderA.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabaseA = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrderA.Id);

            orderInDatabaseA.Should().BeNull();

            Order orderInDatabaseB = await dbContext.Orders.Include(order => order.Parent).FirstWithIdAsync(existingOrderB.Id);

            orderInDatabaseB.Parent.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_referencing_itself()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Orders.Add(existingOrder);
            await dbContext.SaveChangesAsync();

            existingOrder.Parent = existingOrder;
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrder.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            Customer customerInDatabase = await dbContext.Customers.FirstWithIdAsync(existingCustomer.Id);

            customerInDatabase.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_in_cross_table_circular_dependency()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Orders.Add(existingOrder);
            await dbContext.SaveChangesAsync();

            existingCustomer.FirstOrder = existingOrder;
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrder.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Customer customerInDatabase = await dbContext.Customers
                .Include(customer => customer.FirstOrder)
                .Include(customer => customer.Orders)
                .FirstWithIdAsync(existingCustomer.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            customerInDatabase.FirstOrder.Should().BeNull();
            customerInDatabase.Orders.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_multiple_foreign_keys_on_same_entity_where_only_one_matches()
    {
        // Arrange
        Order existingOrderA = _fakers.Order.GenerateOne();
        Customer existingCustomer1 = _fakers.Customer.GenerateOne();
        existingOrderA.Customer = existingCustomer1;

        Order existingOrderB = _fakers.Order.GenerateOne();
        Order existingOrderC = _fakers.Order.GenerateOne();
        Customer existingCustomer2 = _fakers.Customer.GenerateOne();
        existingOrderB.Customer = existingCustomer2;
        existingOrderC.Customer = existingCustomer2;

        Order existingOrderD = _fakers.Order.GenerateOne();
        Order existingOrderE = _fakers.Order.GenerateOne();
        Customer existingCustomer3 = _fakers.Customer.GenerateOne();
        existingOrderD.Customer = existingCustomer3;
        existingOrderE.Customer = existingCustomer3;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Orders.AddRange(existingOrderA, existingOrderB, existingOrderC, existingOrderD, existingOrderE);
            await dbContext.SaveChangesAsync();

            existingCustomer1.FirstOrder = existingOrderA;
            existingCustomer1.LastOrder = existingOrderB;

            existingCustomer2.FirstOrder = existingOrderC;
            existingCustomer2.LastOrder = existingOrderA;

            existingCustomer3.FirstOrder = existingOrderD;
            existingCustomer3.LastOrder = existingOrderE;

            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrderA.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabaseA = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrderA.Id);
            Order? orderInDatabaseB = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrderB.Id);

            orderInDatabaseA.Should().BeNull();
            orderInDatabaseB.Should().NotBeNull();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Customer customerInDatabase1 = await dbContext.Customers
                .Include(customer => customer.FirstOrder)
                .Include(customer => customer.LastOrder)
                .FirstWithIdAsync(existingCustomer1.Id);

            Customer customerInDatabase2 = await dbContext.Customers
                .Include(customer => customer.FirstOrder)
                .Include(customer => customer.LastOrder)
                .FirstWithIdAsync(existingCustomer2.Id);

            Customer customerInDatabase3 = await dbContext.Customers
                .Include(customer => customer.FirstOrder)
                .Include(customer => customer.LastOrder)
                .FirstWithIdAsync(existingCustomer3.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            customerInDatabase1.FirstOrder.Should().BeNull();
            customerInDatabase1.LastOrder.Should().NotBeNull();
            customerInDatabase1.LastOrder.Id.Should().Be(existingOrderB.Id);

            customerInDatabase2.FirstOrder.Should().NotBeNull();
            customerInDatabase2.FirstOrder.Id.Should().Be(existingOrderC.Id);
            customerInDatabase2.LastOrder.Should().BeNull();

            customerInDatabase3.FirstOrder.Should().NotBeNull();
            customerInDatabase3.FirstOrder.Id.Should().Be(existingOrderD.Id);
            customerInDatabase3.LastOrder.Should().NotBeNull();
            customerInDatabase3.LastOrder.Id.Should().Be(existingOrderE.Id);
        });
    }

    [Fact]
    public async Task Can_delete_resource_when_only_subset_of_OneToMany_dependent_records_reference_it()
    {
        // Arrange
        Order existingOrderA = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrderA.Customer = existingCustomer;

        Order existingOrderB = _fakers.Order.GenerateOne();
        existingOrderB.Customer = existingCustomer;

        List<SupportTicket> existingTickets = _fakers.SupportTicket.GenerateList(3);
        existingTickets[0].Order = existingOrderA;
        existingTickets[1].Order = existingOrderA;
        existingTickets[2].Order = existingOrderB;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.SupportTickets.AddRange(existingTickets);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrderA.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabaseA = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrderA.Id);
            Order? orderInDatabaseB = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrderB.Id);

            orderInDatabaseA.Should().BeNull();
            orderInDatabaseB.Should().NotBeNull();

            List<long> ticketIds = existingTickets.Select(ticket => ticket.Id).ToList();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            List<SupportTicket> ticketsInDatabase = await dbContext.SupportTickets
                .Include(ticket => ticket.Order)
                .Where(ticket => ticketIds.Contains(ticket.Id))
                .ToListAsync();

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            ticketsInDatabase.Should().HaveCount(3);
            SupportTicket ticketInDatabase1 = ticketsInDatabase.Single(ticket => ticket.Id == existingTickets[0].Id);
            SupportTicket ticketInDatabase2 = ticketsInDatabase.Single(ticket => ticket.Id == existingTickets[1].Id);
            SupportTicket ticketInDatabase3 = ticketsInDatabase.Single(ticket => ticket.Id == existingTickets[2].Id);

            ticketInDatabase1.Order.Should().BeNull();
            ticketInDatabase2.Order.Should().BeNull();

            ticketInDatabase3.Order.Should().NotBeNull();
            ticketInDatabase3.Order.Id.Should().Be(existingOrderB.Id);
        });
    }

    [Fact]
    public async Task Can_delete_resource_when_only_subset_of_OneToOne_dependent_records_reference_it()
    {
        // Arrange
        Order existingOrderA = _fakers.Order.GenerateOne();
        Order existingOrderB = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrderA.Customer = existingCustomer;
        existingOrderB.Customer = existingCustomer;

        List<ShoppingBasket> existingBaskets = _fakers.ShoppingBasket.GenerateList(3);
        existingBaskets[0].CurrentOrder = existingOrderA;
        existingBaskets[1].CurrentOrder = existingOrderB;
        existingBaskets[2].CurrentOrder = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ShoppingBaskets.AddRange(existingBaskets);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrderA.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabaseA = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrderA.Id);
            Order? orderInDatabaseB = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrderB.Id);

            orderInDatabaseA.Should().BeNull();
            orderInDatabaseB.Should().NotBeNull();

            List<long> basketIds = existingBaskets.Select(basket => basket.Id).ToList();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            List<ShoppingBasket> basketsInDatabase = await dbContext.ShoppingBaskets
                .Include(basket => basket.CurrentOrder)
                .Where(basket => basketIds.Contains(basket.Id))
                .ToListAsync();

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            basketsInDatabase.Should().HaveCount(3);
            ShoppingBasket basketInDatabase1 = basketsInDatabase.Single(basket => basket.Id == existingBaskets[0].Id);
            ShoppingBasket basketInDatabase2 = basketsInDatabase.Single(basket => basket.Id == existingBaskets[1].Id);
            ShoppingBasket basketInDatabase3 = basketsInDatabase.Single(basket => basket.Id == existingBaskets[2].Id);

            basketInDatabase1.CurrentOrder.Should().BeNull();

            basketInDatabase2.CurrentOrder.Should().NotBeNull();
            basketInDatabase2.CurrentOrder.Id.Should().Be(existingOrderB.Id);

            basketInDatabase3.CurrentOrder.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_standalone_resource_without_incoming_references()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Orders.Add(existingOrder);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrder.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            Customer customerInDatabase = await dbContext.Customers.FirstWithIdAsync(existingCustomer.Id);

            customerInDatabase.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_when_no_referencing_foreign_keys_exist()
    {
        // Arrange
        ShoppingBasket existingBasket = _fakers.ShoppingBasket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ShoppingBaskets.Add(existingBasket);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/shoppingBaskets/{existingBasket.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            ShoppingBasket? basketInDatabase = await dbContext.ShoppingBaskets.FirstWithIdOrDefaultAsync(existingBasket.Id);

            basketInDatabase.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_dependent_resource_referencing_principal_with_ClientSetNull()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        ShoppingBasket existingBasket = _fakers.ShoppingBasket.GenerateOne();
        existingBasket.CurrentOrder = existingOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ShoppingBaskets.Add(existingBasket);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/shoppingBaskets/{existingBasket.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            ShoppingBasket? basketInDatabase = await dbContext.ShoppingBaskets.FirstWithIdOrDefaultAsync(existingBasket.Id);

            basketInDatabase.Should().BeNull();

            Order orderInDatabase = await dbContext.Orders.FirstWithIdAsync(existingOrder.Id);

            orderInDatabase.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_referenced_by_not_publicly_exposed_entity()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        OrderAudit existingAudit = _fakers.OrderAudit.GenerateOne();
        existingAudit.Order = existingOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.OrderAudits.Add(existingAudit);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/orders/{existingOrder.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            OrderAudit auditInDatabase = await dbContext.OrderAudits.Include(audit => audit.Order).FirstAsync(audit => audit.Id == existingAudit.Id);

            auditInDatabase.Order.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_ClientSetNull_in_operations_request()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        ShoppingBasket existingBasket = _fakers.ShoppingBasket.GenerateOne();
        existingBasket.CurrentOrder = existingOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ShoppingBaskets.Add(existingBasket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "orders",
                        id = existingOrder.StringId
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

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            ShoppingBasket basketInDatabase = await dbContext.ShoppingBaskets.Include(basket => basket.CurrentOrder).FirstWithIdAsync(existingBasket.Id);

            basketInDatabase.CurrentOrder.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_after_linking_it_in_operations_request()
    {
        // Arrange
        ShoppingBasket existingBasket = _fakers.ShoppingBasket.GenerateOne();

        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ShoppingBaskets.Add(existingBasket);
            dbContext.Orders.Add(existingOrder);
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
                        type = "shoppingBaskets",
                        id = existingBasket.StringId,
                        relationships = new
                        {
                            currentOrder = new
                            {
                                data = new
                                {
                                    type = "orders",
                                    id = existingOrder.StringId
                                }
                            }
                        }
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "orders",
                        id = existingOrder.StringId
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

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            ShoppingBasket basketInDatabase = await dbContext.ShoppingBaskets.Include(basket => basket.CurrentOrder).FirstWithIdAsync(existingBasket.Id);

            basketInDatabase.CurrentOrder.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_before_updating_dependent_in_operations_request()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        ShoppingBasket existingBasket = _fakers.ShoppingBasket.GenerateOne();
        existingBasket.CurrentOrder = existingOrder;

        int newProductCount = existingBasket.ProductCount + 1;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ShoppingBaskets.Add(existingBasket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "orders",
                        id = existingOrder.StringId
                    }
                },
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "shoppingBaskets",
                        id = existingBasket.StringId,
                        attributes = new
                        {
                            productCount = newProductCount
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

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            ShoppingBasket basketInDatabase = await dbContext.ShoppingBaskets.Include(basket => basket.CurrentOrder).FirstWithIdAsync(existingBasket.Id);

            basketInDatabase.CurrentOrder.Should().BeNull();
            basketInDatabase.ProductCount.Should().Be(newProductCount);
        });
    }

    [Fact]
    public async Task Can_delete_multiple_resources_in_operations_request()
    {
        // Arrange
        Order existingOrderA = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrderA.Customer = existingCustomer;

        Order existingOrderB = _fakers.Order.GenerateOne();
        existingOrderB.Customer = existingCustomer;

        SupportTicket existingTicketA = _fakers.SupportTicket.GenerateOne();
        existingTicketA.Order = existingOrderA;

        SupportTicket existingTicketB = _fakers.SupportTicket.GenerateOne();
        existingTicketB.Order = existingOrderB;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.SupportTickets.AddRange(existingTicketA, existingTicketB);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "orders",
                        id = existingOrderA.StringId
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "orders",
                        id = existingOrderB.StringId
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

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabaseA = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrderA.Id);
            Order? orderInDatabaseB = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrderB.Id);

            orderInDatabaseA.Should().BeNull();
            orderInDatabaseB.Should().BeNull();

            SupportTicket ticketInDatabaseA = await dbContext.SupportTickets.Include(ticket => ticket.Order).FirstWithIdAsync(existingTicketA.Id);
            SupportTicket ticketInDatabaseB = await dbContext.SupportTickets.Include(ticket => ticket.Order).FirstWithIdAsync(existingTicketB.Id);

            ticketInDatabaseA.Order.Should().BeNull();
            ticketInDatabaseB.Order.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_both_principal_and_dependent_resources_in_operations_request()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        ShoppingBasket existingBasket = _fakers.ShoppingBasket.GenerateOne();
        existingBasket.CurrentOrder = existingOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ShoppingBaskets.Add(existingBasket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "orders",
                        id = existingOrder.StringId
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "shoppingBaskets",
                        id = existingBasket.StringId
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

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);

            orderInDatabase.Should().BeNull();

            ShoppingBasket? basketInDatabase = await dbContext.ShoppingBaskets.FirstWithIdOrDefaultAsync(existingBasket.Id);

            basketInDatabase.Should().BeNull();
        });
    }
}
