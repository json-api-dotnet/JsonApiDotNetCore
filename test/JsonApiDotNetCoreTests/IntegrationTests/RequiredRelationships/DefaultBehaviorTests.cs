using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
            Order order = _fakers.Orders.Generate();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.Title.Should().Be("An unhandled error occurred while processing this request.");
            error.Detail.Should().Be("Failed to persist changes in the underlying data store.");
        }

        [Fact]
        public async Task Cannot_create_dependent_side_of_required_OneToOne_relationship_without_providing_principal_side()
        {
            // Arrange
            Shipment shipment = _fakers.Shipments.Generate();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.Title.Should().Be("An unhandled error occurred while processing this request.");
            error.Detail.Should().Be("Failed to persist changes in the underlying data store.");
        }

        [Fact]
        public async Task Deleting_principal_side_of_required_OneToMany_relationship_triggers_cascading_delete()
        {
            // Arrange
            Order existingOrder = _fakers.Orders.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

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
                Customer existingCustomerInDatabase = await dbContext.Customers.FirstWithIdOrDefaultAsync(existingOrder.Customer.Id);
                existingCustomerInDatabase.Should().BeNull();

                Order existingOrderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);
                existingOrderInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Deleting_principal_side_of_required_OneToOne_relationship_triggers_cascading_delete()
        {
            // Arrange
            Order existingOrder = _fakers.Orders.Generate();
            existingOrder.Shipment = _fakers.Shipments.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

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
                Order existingOrderInDatabase = await dbContext.Orders.FirstWithIdOrDefaultAsync(existingOrder.Id);
                existingOrderInDatabase.Should().BeNull();

                Shipment existingShipmentInDatabase = await dbContext.Shipments.FirstWithIdOrDefaultAsync(existingOrder.Shipment.Id);
                existingShipmentInDatabase.Should().BeNull();

                Customer existingCustomerInDatabase = await dbContext.Customers.FirstWithIdOrDefaultAsync(existingOrder.Customer.Id);
                existingCustomerInDatabase.Should().NotBeNull();
            });
        }

        [Fact]
        public async Task Cannot_clear_required_ManyToOne_relationship_through_primary_endpoint()
        {
            // Arrange
            Order existingOrder = _fakers.Orders.Generate();
            existingOrder.Shipment = _fakers.Shipments.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    id = existingOrder.StringId,
                    type = "orders",
                    relationships = new
                    {
                        customer = new
                        {
                            data = (object)null
                        }
                    }
                }
            };

            string route = $"/orders/{existingOrder.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Failed to clear a required relationship.");

            error.Detail.Should().Be($"The relationship 'customer' on resource type 'orders' with ID '{existingOrder.StringId}' " +
                "cannot be cleared because it is a required relationship.");
        }

        [Fact]
        public async Task Cannot_clear_required_ManyToOne_relationship_through_relationship_endpoint()
        {
            // Arrange
            Order existingOrder = _fakers.Orders.Generate();
            existingOrder.Shipment = _fakers.Shipments.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = (object)null
            };

            string route = $"/orders/{existingOrder.StringId}/relationships/customer";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Failed to clear a required relationship.");

            error.Detail.Should().Be($"The relationship 'customer' on resource type 'orders' with ID '{existingOrder.StringId}' " +
                "cannot be cleared because it is a required relationship.");
        }

        [Fact]
        public async Task Cannot_clear_required_OneToMany_relationship_through_primary_endpoint()
        {
            // Arrange
            Order existingOrder = _fakers.Orders.Generate();
            existingOrder.Shipment = _fakers.Shipments.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    id = existingOrder.Customer.StringId,
                    type = "customers",
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
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Failed to clear a required relationship.");

            error.Detail.Should().Be($"The relationship 'orders' on resource type 'customers' with ID '{existingOrder.StringId}' " +
                "cannot be cleared because it is a required relationship.");
        }

        [Fact]
        public async Task Cannot_clear_required_OneToMany_relationship_by_updating_through_relationship_endpoint()
        {
            // Arrange
            Order existingOrder = _fakers.Orders.Generate();
            existingOrder.Shipment = _fakers.Shipments.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

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
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Failed to clear a required relationship.");

            error.Detail.Should().Be($"The relationship 'orders' on resource type 'customers' with ID '{existingOrder.StringId}' " +
                "cannot be cleared because it is a required relationship.");
        }

        [Fact]
        public async Task Cannot_clear_required_OneToMany_relationship_by_deleting_through_relationship_endpoint()
        {
            // Arrange
            Order existingOrder = _fakers.Orders.Generate();
            existingOrder.Shipment = _fakers.Shipments.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

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
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Failed to clear a required relationship.");

            error.Detail.Should().Be($"The relationship 'orders' on resource type 'customers' with ID '{existingOrder.StringId}' " +
                "cannot be cleared because it is a required relationship.");
        }

        [Fact]
        public async Task Can_reassign_dependent_side_of_ZeroOrOneToOne_relationship_through_primary_endpoint()
        {
            // Arrange
            Order orderWithShipment = _fakers.Orders.Generate();
            orderWithShipment.Shipment = _fakers.Shipments.Generate();
            orderWithShipment.Customer = _fakers.Customers.Generate();

            Order orderWithoutShipment = _fakers.Orders.Generate();
            orderWithoutShipment.Customer = _fakers.Customers.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.AddRange(orderWithShipment, orderWithoutShipment);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    id = orderWithoutShipment.StringId,
                    type = "orders",
                    relationships = new
                    {
                        shipment = new
                        {
                            data = new
                            {
                                id = orderWithShipment.Shipment.StringId,
                                type = "shipments"
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
                Shipment existingShipmentInDatabase =
                    await dbContext.Shipments.Include(shipment => shipment.Order).FirstWithIdOrDefaultAsync(orderWithShipment.Shipment.Id);

                existingShipmentInDatabase.Order.Id.Should().Be(orderWithoutShipment.Id);
            });
        }

        [Fact]
        public async Task Can_reassign_dependent_side_of_ZeroOrOneToOne_relationship_through_relationship_endpoint()
        {
            // Arrange
            Order orderWithShipment = _fakers.Orders.Generate();
            orderWithShipment.Shipment = _fakers.Shipments.Generate();
            orderWithShipment.Customer = _fakers.Customers.Generate();

            Order orderWithoutShipment = _fakers.Orders.Generate();
            orderWithoutShipment.Customer = _fakers.Customers.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.AddRange(orderWithShipment, orderWithoutShipment);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    id = orderWithShipment.Shipment.StringId,
                    type = "shipments"
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
                Shipment existingShipmentInDatabase =
                    await dbContext.Shipments.Include(shipment => shipment.Order).FirstWithIdOrDefaultAsync(orderWithShipment.Shipment.Id);

                existingShipmentInDatabase.Order.Id.Should().Be(orderWithoutShipment.Id);
            });
        }
    }
}
