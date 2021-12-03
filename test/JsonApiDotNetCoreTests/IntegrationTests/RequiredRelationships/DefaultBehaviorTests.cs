using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships
{
    public sealed class DefaultBehaviorTests : IClassFixture<IntegrationTestContext<TestableStartup<DefaultBehaviorDbContext>, DefaultBehaviorDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<DefaultBehaviorDbContext>, DefaultBehaviorDbContext> _testContext;

        private readonly DefaultBehaviorFakers _fakers = new();

        public DefaultBehaviorTests(IntegrationTestContext<TestableStartup<DefaultBehaviorDbContext>, DefaultBehaviorDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OrdersController>();
            testContext.UseController<ShipmentsController>();
            testContext.UseController<CustomersController>();

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = true;
        }

        [Fact]
        public async Task Cannot_create_dependent_side_of_required_ManyToOne_relationship_without_providing_principal_side()
        {
            // Arrange
            Order order = _fakers.Order.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "orders",
                    attributes = new
                    {
                        amount = order.Amount
                    }
                }
            };

            const string route = "/orders";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(2);

            ErrorObject error1 = responseDocument.Errors[0];
            error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error1.Title.Should().Be("Input validation failed.");
            error1.Detail.Should().Be("The Customer field is required.");
            error1.Source.ShouldNotBeNull();
            error1.Source.Pointer.Should().Be("/data/relationships/customer/data");

            ErrorObject error2 = responseDocument.Errors[1];
            error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error2.Title.Should().Be("Input validation failed.");
            error2.Detail.Should().Be("The Shipment field is required.");
            error2.Source.ShouldNotBeNull();
            error2.Source.Pointer.Should().Be("/data/relationships/shipment/data");
        }

        [Fact]
        public async Task Cannot_create_dependent_side_of_required_OneToOne_relationship_without_providing_principal_side()
        {
            // Arrange
            Shipment shipment = _fakers.Shipment.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "shipments",
                    attributes = new
                    {
                        trackAndTraceCode = shipment.TrackAndTraceCode
                    }
                }
            };

            const string route = "/shipments";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Input validation failed.");
            error.Detail.Should().Be("The Order field is required.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/data/relationships/order/data");
        }

        [Fact]
        public async Task Deleting_principal_side_of_required_OneToMany_relationship_triggers_cascading_delete()
        {
            // Arrange
            Order existingOrder = _fakers.Order.Generate();
            existingOrder.Customer = _fakers.Customer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/customers/{existingOrder.Customer.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Customer? customerInDatabase = await dbContext.Customers.FirstWithIdOrDefaultAsync(existingOrder.Customer.Id);
                customerInDatabase.Should().BeNull();

                Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);
                orderInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Deleting_principal_side_of_required_OneToOne_relationship_triggers_cascading_delete()
        {
            // Arrange
            Order existingOrder = _fakers.Order.Generate();
            existingOrder.Shipment = _fakers.Shipment.Generate();
            existingOrder.Customer = _fakers.Customer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/orders/{existingOrder.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Order? orderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);
                orderInDatabase.Should().BeNull();

                Shipment? shipmentInDatabase = await dbContext.Shipments.FirstWithIdOrDefaultAsync(existingOrder.Shipment.Id);
                shipmentInDatabase.Should().BeNull();

                Customer? customerInDatabase = await dbContext.Customers.FirstWithIdOrDefaultAsync(existingOrder.Customer.Id);
                customerInDatabase.ShouldNotBeNull();
            });
        }

        [Fact]
        public async Task Cannot_clear_required_ManyToOne_relationship_through_primary_endpoint()
        {
            // Arrange
            Order existingOrder = _fakers.Order.Generate();
            existingOrder.Shipment = _fakers.Shipment.Generate();
            existingOrder.Customer = _fakers.Customer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "orders",
                    id = existingOrder.StringId,
                    relationships = new
                    {
                        customer = new
                        {
                            data = (object?)null
                        }
                    }
                }
            };

            string route = $"/orders/{existingOrder.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Input validation failed.");
            error.Detail.Should().Be("The Customer field is required.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/data/relationships/customer/data");
        }

        [Fact]
        public async Task Cannot_clear_required_ManyToOne_relationship_through_relationship_endpoint()
        {
            // Arrange
            Order existingOrder = _fakers.Order.Generate();
            existingOrder.Shipment = _fakers.Shipment.Generate();
            existingOrder.Customer = _fakers.Customer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = (object?)null
            };

            string route = $"/orders/{existingOrder.StringId}/relationships/customer";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Failed to clear a required relationship.");

            error.Detail.Should().Be($"The relationship 'customer' on resource type 'orders' with ID '{existingOrder.StringId}' " +
                "cannot be cleared because it is a required relationship.");
        }

        [Fact]
        public async Task Clearing_OneToMany_relationship_through_primary_endpoint_triggers_cascading_delete()
        {
            // Arrange
            Order existingOrder = _fakers.Order.Generate();
            existingOrder.Shipment = _fakers.Shipment.Generate();
            existingOrder.Customer = _fakers.Customer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "customers",
                    id = existingOrder.Customer.StringId,
                    relationships = new
                    {
                        orders = new
                        {
                            data = Array.Empty<object>()
                        }
                    }
                }
            };

            string route = $"/customers/{existingOrder.Customer.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Order? orderInDatabase = await dbContext.Orders.Include(order => order.Customer).FirstWithIdOrDefaultAsync(existingOrder.Id);
                orderInDatabase.Should().BeNull();

                Customer customerInDatabase = await dbContext.Customers.Include(customer => customer.Orders).FirstWithIdAsync(existingOrder.Customer.Id);
                customerInDatabase.Orders.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Clearing_OneToMany_relationship_through_update_relationship_endpoint_triggers_cascading_delete()
        {
            // Arrange
            Order existingOrder = _fakers.Order.Generate();
            existingOrder.Shipment = _fakers.Shipment.Generate();
            existingOrder.Customer = _fakers.Customer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = Array.Empty<object>()
            };

            string route = $"/customers/{existingOrder.Customer.StringId}/relationships/orders";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Order? orderInDatabase = await dbContext.Orders.Include(order => order.Customer).FirstWithIdOrDefaultAsync(existingOrder.Id);
                orderInDatabase.Should().BeNull();

                Customer customerInDatabase = await dbContext.Customers.Include(customer => customer.Orders).FirstWithIdAsync(existingOrder.Customer.Id);
                customerInDatabase.Orders.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Clearing_OneToMany_relationship_through_delete_relationship_endpoint_triggers_cascading_delete()
        {
            // Arrange
            Order existingOrder = _fakers.Order.Generate();
            existingOrder.Shipment = _fakers.Shipment.Generate();
            existingOrder.Customer = _fakers.Customer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new object[]
                {
                    new
                    {
                        type = "orders",
                        id = existingOrder.StringId
                    }
                }
            };

            string route = $"/customers/{existingOrder.Customer.StringId}/relationships/orders";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Order? orderInDatabase = await dbContext.Orders.Include(order => order.Customer).FirstWithIdOrDefaultAsync(existingOrder.Id);
                orderInDatabase.Should().BeNull();

                Customer customerInDatabase = await dbContext.Customers.Include(customer => customer.Orders).FirstWithIdAsync(existingOrder.Customer.Id);
                customerInDatabase.Orders.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Can_reassign_dependent_side_of_ZeroOrOneToOne_relationship_through_primary_endpoint()
        {
            // Arrange
            Order orderWithShipment = _fakers.Order.Generate();
            orderWithShipment.Shipment = _fakers.Shipment.Generate();
            orderWithShipment.Customer = _fakers.Customer.Generate();

            Order orderWithoutShipment = _fakers.Order.Generate();
            orderWithoutShipment.Customer = _fakers.Customer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.AddRange(orderWithShipment, orderWithoutShipment);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "orders",
                    id = orderWithoutShipment.StringId,
                    relationships = new
                    {
                        shipment = new
                        {
                            data = new
                            {
                                type = "shipments",
                                id = orderWithShipment.Shipment.StringId
                            }
                        }
                    }
                }
            };

            string route = $"/orders/{orderWithoutShipment.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Shipment shipmentInDatabase = await dbContext.Shipments.Include(shipment => shipment.Order).FirstWithIdAsync(orderWithShipment.Shipment.Id);

                shipmentInDatabase.Order.Id.Should().Be(orderWithoutShipment.Id);
            });
        }

        [Fact]
        public async Task Can_reassign_dependent_side_of_ZeroOrOneToOne_relationship_through_relationship_endpoint()
        {
            // Arrange
            Order orderWithShipment = _fakers.Order.Generate();
            orderWithShipment.Shipment = _fakers.Shipment.Generate();
            orderWithShipment.Customer = _fakers.Customer.Generate();

            Order orderWithoutShipment = _fakers.Order.Generate();
            orderWithoutShipment.Customer = _fakers.Customer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.AddRange(orderWithShipment, orderWithoutShipment);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "shipments",
                    id = orderWithShipment.Shipment.StringId
                }
            };

            string route = $"/orders/{orderWithoutShipment.StringId}/relationships/shipment";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Shipment shipmentInDatabase = await dbContext.Shipments.Include(shipment => shipment.Order).FirstWithIdAsync(orderWithShipment.Shipment.Id);

                shipmentInDatabase.Order.Id.Should().Be(orderWithoutShipment.Id);
            });
        }
    }
}
