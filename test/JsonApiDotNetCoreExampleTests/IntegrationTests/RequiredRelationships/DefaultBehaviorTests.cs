using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class DefaultBehaviorTests : IClassFixture<IntegrationTestContext<TestableStartup<DefaultBehaviorDbContext>, DefaultBehaviorDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<DefaultBehaviorDbContext>, DefaultBehaviorDbContext> _testContext;

        private readonly DefaultBehaviorFakers _fakers = new DefaultBehaviorFakers();

        public DefaultBehaviorTests(IntegrationTestContext<TestableStartup<DefaultBehaviorDbContext>, DefaultBehaviorDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = true;
        }

        [Fact]
        public async Task Cannot_create_dependent_side_of_required_OneToMany_relationship_without_providing_principal_side()
        {
            // Arrange
            var order = _fakers.Orders.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "orders",
                    attributes = new
                    {
                        order = order.Amount
                    }
                }
            };

            var route = "/orders";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.GetErrorStatusCode().Should().Be(HttpStatusCode.InternalServerError);
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].Title.Should().Be("An unhandled error occurred while processing this request.");
            responseDocument.Errors[0].Detail.Should().Be("Failed to persist changes in the underlying data store.");
        }

        [Fact]
        public async Task Cannot_create_dependent_side_of_required_OneToOne_relationship_without_providing_principal_side()
        {
            // Arrange
            var shipment = _fakers.Shipments.Generate();

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

            var route = "/shipments";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.GetErrorStatusCode().Should().Be(HttpStatusCode.InternalServerError);
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].Title.Should().Be("An unhandled error occurred while processing this request.");
            responseDocument.Errors[0].Detail.Should().Be("Failed to persist changes in the underlying data store.");
        }

        [Fact]
        public async Task Cannot_create_dependent_side_of_required_OneToMany_relationship_with_unknown_principal_id()
        {
            // Arrange
            var order = _fakers.Orders.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "orders",
                    attributes = new
                    {
                        order = order.Amount
                    },
                    relationships = new
                    {
                        customer = new
                        {
                            data = new
                            {
                                id = "999999",
                                type = "customers"
                            }
                        }
                    }
                }
            };

            var route = "/orders";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A related resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Related resource of type 'customers' with ID '999999' in relationship 'customer' does not exist.");
        }

        [Fact]
        public async Task Cannot_create_dependent_side_of_required_OneToOne_relationship_with_unknown_principal_id()
        {
            // Arrange
            var shipment = _fakers.Shipments.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "shipments",
                    attributes = new
                    {
                        trackAndTraceCode = shipment.TrackAndTraceCode
                    },
                    relationships = new
                    {
                        order = new
                        {
                            data = new
                            {
                                id = "999999",
                                type = "orders"
                            }
                        }
                    }
                }
            };

            var route = "/shipments";

            // Act
            var (httpResponse, responseDocument) =
                await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A related resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Related resource of type 'orders' with ID '999999' in relationship 'order' does not exist.");
        }

        [Fact]
        public async Task Deleting_principal_side_of_required_OneToMany_relationship_triggers_cascade_deletion()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/customers/{existingOrder.Customer.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var existingCustomerInDatabase = await dbContext.Customers.FindAsync(existingOrder.Customer.Id);
                existingCustomerInDatabase.Should().BeNull();

                var existingOrderInDatabase = await dbContext.Orders.FindAsync(existingOrder.Id);
                existingOrderInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Deleting_principal_side_of_required_OneToOne_relationship_triggers_cascade_deletion()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
            existingOrder.Shipment = _fakers.Shipments.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/orders/{existingOrder.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var existingOrderInDatabase = await dbContext.Orders.FindAsync(existingOrder.Id);
                existingOrderInDatabase.Should().BeNull();

                var existingShipmentInDatabase = await dbContext.Shipments.FindAsync(existingOrder.Shipment.Id);
                existingShipmentInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Cannot_clear_required_OneToOne_relationship_from_principal_side()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
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
                    id = existingOrder.Id,
                    type = "orders",
                    relationships = new
                    {
                        shipment = new
                        {
                            data = (object) null
                        }
                    }
                }
            };

            var route = $"/orders/{existingOrder.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.GetErrorStatusCode().Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].Title.Should().Be("A required relationship cannot be cleared.");
            responseDocument.Errors[0].Detail.Should().Be("The required relationship 'shipment' of resource of type 'orders' cannot be cleared.");
        }

        [Fact]
        public async Task Cannot_clear_required_OneToOne_relationship_from_dependent_side()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
            existingOrder.Shipment = _fakers.Shipments.Generate();
            existingOrder.Customer = _fakers.Customers.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Orders.Add(existingOrder);
                await dbContext.SaveChangesAsync();
            });

            await _testContext.RunOnDatabaseAsync(async dbContext => { await dbContext.SaveChangesAsync(); });

            var requestBody = new
            {
                data = new
                {
                    id = existingOrder.Shipment.Id,
                    type = "shipments",
                    relationships = new
                    {
                        order = new
                        {
                            data = (object) null
                        }
                    }
                }
            };

            var route = $"/shipments/{existingOrder.Shipment.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.GetErrorStatusCode().Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].Title.Should().Be("A required relationship cannot be cleared.");
            responseDocument.Errors[0].Detail.Should().Be("The required relationship 'order' of resource of type 'shipments' cannot be cleared.");
        }

        [Fact]
        public async Task Cannot_clear_required_OneToMany_relationship_from_dependent_side()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
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
                    id = existingOrder.Id,
                    type = "orders",
                    relationships = new
                    {
                        customer = new
                        {
                            data = (object) null
                        }
                    }
                }
            };

            var route = $"/orders/{existingOrder.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.GetErrorStatusCode().Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].Title.Should().Be("A required relationship cannot be cleared.");
            responseDocument.Errors[0].Detail.Should().Be("The required relationship 'customer' of resource of type 'orders' cannot be cleared.");
        }

        [Fact]
        public async Task Cannot_clear_required_OneToMany_relationship_from_principal_side()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
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
                    id = existingOrder.Customer.Id,
                    type = "customers",
                    relationships = new
                    {
                        orders = new
                        {
                            data = new object[0]
                        }
                    }
                }
            };

            var route = $"/customers/{existingOrder.Customer.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.GetErrorStatusCode().Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].Title.Should().Be("A required relationship cannot be cleared.");
            responseDocument.Errors[0].Detail.Should().Be("The required relationship 'orders' of resource of type 'customers' cannot be cleared.");
        }

        [Fact]
        public async Task Cannot_reassign_dependent_side_of_OneToOne_relationship_with_identifying_foreign_key()
        {
            // Arrange
            var orderWithShipment = _fakers.Orders.Generate();
            orderWithShipment.Shipment = _fakers.Shipments.Generate();
            orderWithShipment.Customer = _fakers.Customers.Generate();

            var orderWithoutShipment = _fakers.Orders.Generate();
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
                    id = orderWithoutShipment.Id,
                    type = "orders",
                    relationships = new
                    {
                        shipment = new
                        {
                            data = new
                            {
                                id = orderWithShipment.Shipment.Id,
                                type = "shipments"
                            }
                        }
                    }
                }
            };

            var route = $"/orders/{orderWithoutShipment.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.GetErrorStatusCode().Should().Be(HttpStatusCode.InternalServerError);
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].Title.Should().Be("An unhandled error occurred while processing this request.");
            responseDocument.Errors[0].Detail.Should().StartWith("The property 'Id' on entity type 'Shipment' is part of a key and so cannot be modified or marked as modified.");
        }
    }
}
