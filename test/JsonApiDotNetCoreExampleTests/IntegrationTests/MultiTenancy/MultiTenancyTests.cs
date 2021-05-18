using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.MultiTenancy
{
    public sealed class MultiTenancyTests : IClassFixture<ExampleIntegrationTestContext<TestableStartup<MultiTenancyDbContext>, MultiTenancyDbContext>>
    {
        private static readonly Guid ThisTenantId = Guid.NewGuid();
        private static readonly Guid OtherTenantId = Guid.NewGuid();

        private readonly ExampleIntegrationTestContext<TestableStartup<MultiTenancyDbContext>, MultiTenancyDbContext> _testContext;
        private readonly MultiTenancyFakers _fakers = new MultiTenancyFakers();

        public MultiTenancyTests(ExampleIntegrationTestContext<TestableStartup<MultiTenancyDbContext>, MultiTenancyDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<WebShopsController>();
            testContext.UseController<WebProductsController>();

            testContext.ConfigureServicesBeforeStartup(services =>
            {
                services.AddSingleton<ITenantProvider>(new FakeTenantProvider(ThisTenantId));
            });

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceService<MultiTenantResourceService<WebShop>>();
                services.AddResourceService<MultiTenantResourceService<WebProduct>>();
            });
        }

        [Fact]
        public async Task Get_primary_resources_hides_other_tenants()
        {
            // Arrange
            List<WebShop> shops = _fakers.WebShop.Generate(2);
            shops[0].TenantId = OtherTenantId;
            shops[1].TenantId = ThisTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<WebShop>();
                dbContext.WebShops.AddRange(shops);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/webShops";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(shops[1].StringId);
        }

        [Fact]
        public async Task Filter_on_primary_resources_hides_other_tenants()
        {
            // Arrange
            List<WebShop> shops = _fakers.WebShop.Generate(2);
            shops[0].TenantId = OtherTenantId;
            shops[0].Products = _fakers.WebProduct.Generate(1);

            shops[1].TenantId = ThisTenantId;
            shops[1].Products = _fakers.WebProduct.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<WebShop>();
                dbContext.WebShops.AddRange(shops);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/webShops?filter=has(products)";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(shops[1].StringId);
        }

        [Fact]
        public async Task Get_primary_resources_with_include_hides_other_tenants()
        {
            // Arrange
            List<WebShop> shops = _fakers.WebShop.Generate(2);
            shops[0].TenantId = OtherTenantId;
            shops[0].Products = _fakers.WebProduct.Generate(1);

            shops[1].TenantId = ThisTenantId;
            shops[1].Products = _fakers.WebProduct.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<WebShop>();
                dbContext.WebShops.AddRange(shops);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/webShops?include=products";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Type.Should().Be("webShops");
            responseDocument.ManyData[0].Id.Should().Be(shops[1].StringId);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("webProducts");
            responseDocument.Included[0].Id.Should().Be(shops[1].Products[0].StringId);
        }

        [Fact]
        public async Task Cannot_get_primary_resource_by_ID_from_other_tenant()
        {
            // Arrange
            WebShop shop = _fakers.WebShop.Generate();
            shop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebShops.Add(shop);
                await dbContext.SaveChangesAsync();
            });

            string route = "/webShops/" + shop.StringId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webShops' with ID '{shop.StringId}' does not exist.");
        }

        [Fact]
        public async Task Cannot_get_secondary_resources_from_other_parent_tenant()
        {
            // Arrange
            WebShop shop = _fakers.WebShop.Generate();
            shop.TenantId = OtherTenantId;
            shop.Products = _fakers.WebProduct.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebShops.Add(shop);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/webShops/{shop.StringId}/products";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webShops' with ID '{shop.StringId}' does not exist.");
        }

        [Fact]
        public async Task Cannot_get_secondary_resource_from_other_parent_tenant()
        {
            // Arrange
            WebProduct product = _fakers.WebProduct.Generate();
            product.Shop = _fakers.WebShop.Generate();
            product.Shop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebProducts.Add(product);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/webProducts/{product.StringId}/shop";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webProducts' with ID '{product.StringId}' does not exist.");
        }

        [Fact]
        public async Task Cannot_get_HasMany_relationship_for_other_parent_tenant()
        {
            // Arrange
            WebShop shop = _fakers.WebShop.Generate();
            shop.TenantId = OtherTenantId;
            shop.Products = _fakers.WebProduct.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebShops.Add(shop);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/webShops/{shop.StringId}/relationships/products";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webShops' with ID '{shop.StringId}' does not exist.");
        }

        [Fact]
        public async Task Cannot_get_HasOne_relationship_for_other_parent_tenant()
        {
            // Arrange
            WebProduct product = _fakers.WebProduct.Generate();
            product.Shop = _fakers.WebShop.Generate();
            product.Shop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebProducts.Add(product);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/webProducts/{product.StringId}/relationships/shop";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webProducts' with ID '{product.StringId}' does not exist.");
        }

        [Fact]
        public async Task Can_create_resource()
        {
            // Arrange
            string newShopUrl = _fakers.WebShop.Generate().Url;

            var requestBody = new
            {
                data = new
                {
                    type = "webShops",
                    attributes = new
                    {
                        url = newShopUrl
                    }
                }
            };

            const string route = "/webShops";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes["url"].Should().Be(newShopUrl);
            responseDocument.SingleData.Relationships.Should().NotBeNull();

            int newShopId = int.Parse(responseDocument.SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                WebShop shopInDatabase = await dbContext.WebShops.FirstWithIdAsync(newShopId);

                shopInDatabase.Url.Should().Be(newShopUrl);
                shopInDatabase.TenantId.Should().Be(ThisTenantId);
            });
        }

        [Fact]
        public async Task Cannot_create_resource_with_HasMany_relationship_to_other_tenant()
        {
            // Arrange
            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = OtherTenantId;

            string newShopUrl = _fakers.WebShop.Generate().Url;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebProducts.Add(existingProduct);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "webShops",
                    attributes = new
                    {
                        url = newShopUrl
                    },
                    relationships = new
                    {
                        products = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "webProducts",
                                    id = existingProduct.StringId
                                }
                            }
                        }
                    }
                }
            };

            const string route = "/webShops";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("A related resource does not exist.");
            error.Detail.Should().Be($"Related resource of type 'webProducts' with ID '{existingProduct.StringId}' in relationship 'products' does not exist.");
        }

        [Fact]
        public async Task Cannot_create_resource_with_HasOne_relationship_to_other_tenant()
        {
            // Arrange
            WebShop existingShop = _fakers.WebShop.Generate();
            existingShop.TenantId = OtherTenantId;

            string newProductName = _fakers.WebProduct.Generate().Name;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebShops.Add(existingShop);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "webProducts",
                    attributes = new
                    {
                        name = newProductName
                    },
                    relationships = new
                    {
                        shop = new
                        {
                            data = new
                            {
                                type = "webShops",
                                id = existingShop.StringId
                            }
                        }
                    }
                }
            };

            const string route = "/webProducts";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("A related resource does not exist.");
            error.Detail.Should().Be($"Related resource of type 'webShops' with ID '{existingShop.StringId}' in relationship 'shop' does not exist.");
        }

        [Fact]
        public async Task Can_update_resource()
        {
            // Arrange
            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = ThisTenantId;

            string newProductName = _fakers.WebProduct.Generate().Name;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebProducts.Add(existingProduct);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "webProducts",
                    id = existingProduct.StringId,
                    attributes = new
                    {
                        name = newProductName
                    }
                }
            };

            string route = "/webProducts/" + existingProduct.StringId;

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                WebProduct productInDatabase = await dbContext.WebProducts.FirstWithIdAsync(existingProduct.Id);

                productInDatabase.Name.Should().Be(newProductName);
                productInDatabase.Price.Should().Be(existingProduct.Price);
            });
        }

        [Fact]
        public async Task Cannot_update_resource_from_other_tenant()
        {
            // Arrange
            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = OtherTenantId;

            string newProductName = _fakers.WebProduct.Generate().Name;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebProducts.Add(existingProduct);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "webProducts",
                    id = existingProduct.StringId,
                    attributes = new
                    {
                        name = newProductName
                    }
                }
            };

            string route = "/webProducts/" + existingProduct.StringId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webProducts' with ID '{existingProduct.StringId}' does not exist.");
        }

        [Fact]
        public async Task Cannot_update_resource_with_HasMany_relationship_to_other_tenant()
        {
            // Arrange
            WebShop existingShop = _fakers.WebShop.Generate();
            existingShop.TenantId = ThisTenantId;

            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingShop, existingProduct);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "webShops",
                    id = existingShop.StringId,
                    relationships = new
                    {
                        products = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "webProducts",
                                    id = existingProduct.StringId
                                }
                            }
                        }
                    }
                }
            };

            string route = "/webShops/" + existingShop.StringId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("A related resource does not exist.");
            error.Detail.Should().Be($"Related resource of type 'webProducts' with ID '{existingProduct.StringId}' in relationship 'products' does not exist.");
        }

        [Fact]
        public async Task Cannot_update_resource_with_HasOne_relationship_to_other_tenant()
        {
            // Arrange
            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = ThisTenantId;

            WebShop existingShop = _fakers.WebShop.Generate();
            existingShop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingProduct, existingShop);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "webProducts",
                    id = existingProduct.StringId,
                    relationships = new
                    {
                        shop = new
                        {
                            data = new
                            {
                                type = "webShops",
                                id = existingShop.StringId
                            }
                        }
                    }
                }
            };

            string route = "/webProducts/" + existingProduct.StringId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("A related resource does not exist.");
            error.Detail.Should().Be($"Related resource of type 'webShops' with ID '{existingShop.StringId}' in relationship 'shop' does not exist.");
        }

        [Fact]
        public async Task Cannot_update_HasMany_relationship_for_other_parent_tenant()
        {
            // Arrange
            WebShop existingShop = _fakers.WebShop.Generate();
            existingShop.TenantId = OtherTenantId;
            existingShop.Products = _fakers.WebProduct.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebShops.Add(existingShop);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new object[0]
            };

            string route = $"/webShops/{existingShop.StringId}/relationships/products";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webShops' with ID '{existingShop.StringId}' does not exist.");
        }

        [Fact]
        public async Task Cannot_update_HasMany_relationship_to_other_tenant()
        {
            // Arrange
            WebShop existingShop = _fakers.WebShop.Generate();
            existingShop.TenantId = ThisTenantId;

            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingShop, existingProduct);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "webProducts",
                        id = existingProduct.StringId
                    }
                }
            };

            string route = $"/webShops/{existingShop.StringId}/relationships/products";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("A related resource does not exist.");
            error.Detail.Should().Be($"Related resource of type 'webProducts' with ID '{existingProduct.StringId}' in relationship 'products' does not exist.");
        }

        [Fact]
        public async Task Cannot_update_HasOne_relationship_for_other_parent_tenant()
        {
            // Arrange
            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebProducts.Add(existingProduct);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = (object)null
            };

            string route = $"/webProducts/{existingProduct.StringId}/relationships/shop";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webProducts' with ID '{existingProduct.StringId}' does not exist.");
        }

        [Fact]
        public async Task Cannot_update_HasOne_relationship_to_other_tenant()
        {
            // Arrange
            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = ThisTenantId;

            WebShop existingShop = _fakers.WebShop.Generate();
            existingShop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingProduct, existingShop);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "webShops",
                    id = existingShop.StringId
                }
            };

            string route = $"/webProducts/{existingProduct.StringId}/relationships/shop";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("A related resource does not exist.");
            error.Detail.Should().Be($"Related resource of type 'webShops' with ID '{existingShop.StringId}' in relationship 'shop' does not exist.");
        }

        [Fact]
        public async Task Cannot_add_to_ToMany_relationship_for_other_parent_tenant()
        {
            // Arrange
            WebShop existingShop = _fakers.WebShop.Generate();
            existingShop.TenantId = OtherTenantId;

            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = ThisTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingShop, existingProduct);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "webProducts",
                        id = existingProduct.StringId
                    }
                }
            };

            string route = $"/webShops/{existingShop.StringId}/relationships/products";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webShops' with ID '{existingShop.StringId}' does not exist.");
        }

        [Fact]
        public async Task Cannot_add_to_ToMany_relationship_with_other_tenant()
        {
            WebShop existingShop = _fakers.WebShop.Generate();
            existingShop.TenantId = ThisTenantId;

            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingShop, existingProduct);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "webProducts",
                        id = existingProduct.StringId
                    }
                }
            };

            string route = $"/webShops/{existingShop.StringId}/relationships/products";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("A related resource does not exist.");
            error.Detail.Should().Be($"Related resource of type 'webProducts' with ID '{existingProduct.StringId}' in relationship 'products' does not exist.");
        }

        [Fact]
        public async Task Cannot_remove_from_ToMany_relationship_for_other_parent_tenant()
        {
            // Arrange
            WebShop existingShop = _fakers.WebShop.Generate();
            existingShop.TenantId = OtherTenantId;
            existingShop.Products = _fakers.WebProduct.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebShops.Add(existingShop);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "webProducts",
                        id = existingShop.Products[0].StringId
                    }
                }
            };

            string route = $"/webShops/{existingShop.StringId}/relationships/products";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webShops' with ID '{existingShop.StringId}' does not exist.");
        }

        [Fact]
        public async Task Can_delete_resource()
        {
            // Arrange
            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = ThisTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebProducts.Add(existingProduct);
                await dbContext.SaveChangesAsync();
            });

            string route = "/webProducts/" + existingProduct.StringId;

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                WebProduct productInDatabase = await dbContext.WebProducts.FirstWithIdOrDefaultAsync(existingProduct.Id);

                productInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Cannot_delete_resource_from_other_tenant()
        {
            // Arrange
            WebProduct existingProduct = _fakers.WebProduct.Generate();
            existingProduct.Shop = _fakers.WebShop.Generate();
            existingProduct.Shop.TenantId = OtherTenantId;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WebProducts.Add(existingProduct);
                await dbContext.SaveChangesAsync();
            });

            string route = "/webProducts/" + existingProduct.StringId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'webProducts' with ID '{existingProduct.StringId}' does not exist.");
        }
    }
}
