using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete;

public sealed class ClientSetNullOnDeleteTests
    : IClassFixture<IntegrationTestContext<TestableStartup<ClientSetNullOnDeleteDbContext>, ClientSetNullOnDeleteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ClientSetNullOnDeleteDbContext>, ClientSetNullOnDeleteDbContext> _testContext;
    private readonly ClientSetNullOnDeleteFakers _fakers = new();

    public ClientSetNullOnDeleteTests(IntegrationTestContext<TestableStartup<ClientSetNullOnDeleteDbContext>, ClientSetNullOnDeleteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OrdersController>();
        testContext.UseController<ShoppingBasketsController>();
        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task Can_delete_resource_with_multiple_incoming_optional_OneToOne_relationships_from_different_entities()
    {
        // Arrange
        Order existingOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrder.Customer = existingCustomer;

        List<ShoppingBasket> existingBaskets = _fakers.ShoppingBasket.GenerateList(2);
        existingBaskets[0].CurrentOrder = existingOrder;
        existingBaskets[1].CurrentOrder = existingOrder;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ShoppingBaskets.AddRange(existingBaskets);
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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Customer customerInDatabase = await dbContext.Customers
                .Include(customer => customer.FirstOrder)
                .Include(customer => customer.LastOrder)
                .FirstWithIdAsync(existingCustomer.Id);

            List<ShoppingBasket> basketsInDatabase = await dbContext.ShoppingBaskets
                .Include(basket => basket.CurrentOrder)
                .Where(basket => basket.Id == existingBaskets[0].Id || basket.Id == existingBaskets[1].Id)
                .ToListAsync();

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabase.Should().BeNull();

            customerInDatabase.FirstOrder.Should().BeNull();
            customerInDatabase.LastOrder.Should().BeNull();

            basketsInDatabase.Should().HaveCount(2);
            basketsInDatabase.Should().OnlyContain(basket => basket.CurrentOrder == null);
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_self_referencing_OneToOne_relationship()
    {
        // Arrange
        Order existingParentOrder = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingParentOrder.Customer = existingCustomer;

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
            Order? parentOrderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingParentOrder.Id);

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Order childOrderInDatabase = await dbContext.Orders
                .Include(order => order.Parent)
                .FirstWithIdAsync(existingChildOrder.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            parentOrderInDatabase.Should().BeNull();

            childOrderInDatabase.Parent.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_circular_self_referencing_OneToOne_relationship()
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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Order orderInDatabaseB = await dbContext.Orders
                .Include(order => order.Parent)
                .FirstWithIdAsync(existingOrderB.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabaseA.Should().BeNull();
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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Customer customerInDatabase = await dbContext.Customers
                .Include(customer => customer.FirstOrder)
                .FirstWithIdAsync(existingCustomer.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabase.Should().BeNull();

            customerInDatabase.FirstOrder.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_multiple_foreign_keys_on_same_entity_where_only_one_matches()
    {
        // Arrange
        Order existingOrderA = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrderA.Customer = existingCustomer;

        Order existingOrderB = _fakers.Order.GenerateOne();
        existingOrderB.Customer = existingCustomer;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Orders.AddRange(existingOrderA, existingOrderB);
            await dbContext.SaveChangesAsync();

            existingCustomer.FirstOrder = existingOrderA;
            existingCustomer.LastOrder = existingOrderB;
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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Customer customerInDatabase = await dbContext.Customers
                .Include(customer => customer.FirstOrder)
                .Include(customer => customer.LastOrder)
                .FirstWithIdAsync(existingCustomer.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabaseA.Should().BeNull();
            orderInDatabaseB.Should().NotBeNull();

            customerInDatabase.FirstOrder.Should().BeNull();
            customerInDatabase.LastOrder.Should().NotBeNull();
            customerInDatabase.LastOrder.Id.Should().Be(existingOrderB.Id);
        });
    }

    [Fact]
    public async Task Can_delete_resource_when_only_subset_of_dependent_records_reference_it()
    {
        // Arrange
        Order existingOrderA = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrderA.Customer = existingCustomer;

        Order existingOrderB = _fakers.Order.GenerateOne();
        existingOrderB.Customer = existingCustomer;

        List<ShoppingBasket> existingBaskets = _fakers.ShoppingBasket.GenerateList(3);
        existingBaskets[0].CurrentOrder = existingOrderA;
        existingBaskets[1].CurrentOrder = existingOrderA;
        existingBaskets[2].CurrentOrder = existingOrderB;

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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            List<ShoppingBasket> basketsInDatabase = await dbContext.ShoppingBaskets
                .Include(basket => basket.CurrentOrder)
                .Where(basket => existingBaskets.Select(shoppingBasket => shoppingBasket.Id)
                    .Contains(basket.Id))
                .ToListAsync();

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabaseA.Should().BeNull();
            orderInDatabaseB.Should().NotBeNull();

            ShoppingBasket basketInDatabase1 = basketsInDatabase.Single(basket => basket.Id == existingBaskets[0].Id);
            ShoppingBasket basketInDatabase2 = basketsInDatabase.Single(basket => basket.Id == existingBaskets[1].Id);
            ShoppingBasket basketInDatabase3 = basketsInDatabase.Single(basket => basket.Id == existingBaskets[2].Id);

            basketInDatabase1.CurrentOrder.Should().BeNull();
            basketInDatabase2.CurrentOrder.Should().BeNull();
            basketInDatabase3.CurrentOrder.Should().NotBeNull();
            basketInDatabase3.CurrentOrder.Id.Should().Be(existingOrderB.Id);
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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            OrderAudit auditInDatabase = await dbContext.OrderAudits
                .Include(audit => audit.Order)
                .FirstAsync(audit => audit.Id == existingAudit.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabase.Should().BeNull();

            auditInDatabase.Order.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_ClientSetNull_in_atomic_operations()
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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            ShoppingBasket basketInDatabase = await dbContext.ShoppingBaskets
                .Include(basket => basket.CurrentOrder)
                .FirstWithIdAsync(existingBasket.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabase.Should().BeNull();

            basketInDatabase.CurrentOrder.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_in_multi_operation_batch_after_linking_it()
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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            ShoppingBasket basketInDatabase = await dbContext.ShoppingBaskets
                .Include(basket => basket.CurrentOrder)
                .FirstWithIdAsync(existingBasket.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabase.Should().BeNull();

            basketInDatabase.CurrentOrder.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_delete_resource_in_multi_operation_batch_before_updating_dependent()
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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            ShoppingBasket basketInDatabase = await dbContext.ShoppingBaskets
                .Include(basket => basket.CurrentOrder)
                .FirstWithIdAsync(existingBasket.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabase.Should().BeNull();

            basketInDatabase.CurrentOrder.Should().BeNull();
            basketInDatabase.ProductCount.Should().Be(newProductCount);
        });
    }

    [Fact]
    public async Task Can_delete_multiple_resources_in_single_atomic_operations_batch()
    {
        // Arrange
        Order existingOrderA = _fakers.Order.GenerateOne();
        Customer existingCustomer = _fakers.Customer.GenerateOne();
        existingOrderA.Customer = existingCustomer;

        Order existingOrderB = _fakers.Order.GenerateOne();
        existingOrderB.Customer = existingCustomer;

        ShoppingBasket existingBasketA = _fakers.ShoppingBasket.GenerateOne();
        existingBasketA.CurrentOrder = existingOrderA;

        ShoppingBasket existingBasketB = _fakers.ShoppingBasket.GenerateOne();
        existingBasketB.CurrentOrder = existingOrderB;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ShoppingBaskets.AddRange(existingBasketA, existingBasketB);
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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            ShoppingBasket basketInDatabaseA = await dbContext.ShoppingBaskets
                .Include(basket => basket.CurrentOrder)
                .FirstWithIdAsync(existingBasketA.Id);

            ShoppingBasket basketInDatabaseB = await dbContext.ShoppingBaskets
                .Include(basket => basket.CurrentOrder)
                .FirstWithIdAsync(existingBasketB.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            orderInDatabaseA.Should().BeNull();
            orderInDatabaseB.Should().BeNull();
            basketInDatabaseA.CurrentOrder.Should().BeNull();
            basketInDatabaseB.CurrentOrder.Should().BeNull();
        });
    }
}
