using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class RequiredRelationshipsTests : IClassFixture<IntegrationTestContext<
        TestableStartup<RequiredRelationshipsDbContext>, RequiredRelationshipsDbContext>>
    {
        private readonly
            IntegrationTestContext<TestableStartup<RequiredRelationshipsDbContext>, RequiredRelationshipsDbContext>
            _testContext;

        private readonly RequiredRelationshipFakers _fakers = new RequiredRelationshipFakers();

        public RequiredRelationshipsTests(
            IntegrationTestContext<TestableStartup<RequiredRelationshipsDbContext>, RequiredRelationshipsDbContext>
                testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = true;
        }

        // TODO: Consider throwing a more informative error.
        [Fact]
        public async Task
            Cannot_create_dependent_side_of_required_OneToMany_relationship_without_providing_principal_side()
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
                        order = order.Value
                    }
                }
            };

            var route = "/orders";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        }

        // TODO: Consider throwing a more informative error.
        [Fact]
        public async Task Cannot_dependent_side_of_required_OneToOne_relationship_without_providing_principal_side()
        {
            // Arrange
            var delivery = _fakers.Deliveries.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "deliveries",
                    attributes = new
                    {
                        trackAndTraceCode = delivery.TrackAndTraceCode
                    }
                }
            };

            var route = "/deliveries";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
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
                        order = order.Value
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
            var (httpResponse, responseDocument) =
                await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A related resource does not exist.");
            responseDocument.Errors[0].Detail.Should()
                .Be("Related resource of type 'customers' with ID '999999' in relationship 'customer' does not exist.");
        }

        [Fact]
        public async Task Cannot_create_dependent_side_of_required_OneToOne_relationship_with_unknown_principal_id()
        {
            // Arrange
            var delivery = _fakers.Deliveries.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "deliveries",
                    attributes = new
                    {
                        trackAndTraceCode = delivery.TrackAndTraceCode
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

            var route = "/deliveries";

            // Act
            var (httpResponse, responseDocument) =
                await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A related resource does not exist.");
            responseDocument.Errors[0].Detail.Should()
                .Be("Related resource of type 'orders' with ID '999999' in relationship 'order' does not exist.");
        }

        [Fact]
        public async Task Can_delete_principal_side_of_required_OneToMany_relationship()
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
        public async Task Can_delete_principal_side_of_required_OneToOne_relationship()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
            existingOrder.Delivery = _fakers.Deliveries.Generate();
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

                var existingDeliveryInDatabase = await dbContext.Deliveries.FindAsync(existingOrder.Delivery.Id);
                existingDeliveryInDatabase.Should().BeNull();
            });
        }

        // TODO: clearing required relationships is ignored by EF Core. We should throw an error.
        [Fact]
        public async Task Cannot_clear_required_OneToOne_relationship_from_principal_side()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
            existingOrder.Delivery = _fakers.Deliveries.Generate();
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
                        delivery = new
                        {
                            data = (object) null
                        }
                    }
                }
            };

            var route = $"/orders/{existingOrder.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Cannot_clear_required_OneToOne_relationship_from_dependent_side()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
            existingOrder.Delivery = _fakers.Deliveries.Generate();
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
                    type = "deliveries",
                    relationships = new
                    {
                        order = new
                        {
                            data = (object) null
                        }
                    }
                }
            };

            var route = $"/deliveries/{existingOrder.Delivery.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        }

        // TODO: clearing required relationships is ignored by EF Core. We should throw an error.
        [Fact]
        public async Task Cannot_clear_required_OneToMany_relationship_from_dependent_side()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
            existingOrder.Delivery = _fakers.Deliveries.Generate();
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
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Cannot_clear_required_OneToMany_relationship_from_principal_side()
        {
            // Arrange
            var existingOrder = _fakers.Orders.Generate();
            existingOrder.Delivery = _fakers.Deliveries.Generate();
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
                    type = "customers",
                    relationships = new
                    {
                        order = new
                        {
                            data = new object[0]
                        }
                    }
                }
            };

            var route = $"/customers/{existingOrder.Customer.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        }
    }
}
