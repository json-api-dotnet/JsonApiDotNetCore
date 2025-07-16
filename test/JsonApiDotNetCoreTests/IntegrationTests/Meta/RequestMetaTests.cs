using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request.Adapters;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

public sealed class RequestMetaTests : IClassFixture<IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> _testContext;
    private readonly MetaFakers _fakers = new();

    public RequestMetaTests(IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ProductFamiliesController>();
        testContext.UseController<SupportTicketsController>();
        testContext.UseController<OperationsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IResponseMeta, SupportResponseMeta>();
            services.AddSingleton<RequestDocumentStore>();
            services.AddScoped<DocumentAdapter>();

            services.AddScoped<IDocumentAdapter>(serviceProvider =>
            {
                var documentAdapter = serviceProvider.GetRequiredService<DocumentAdapter>();
                var requestDocumentStore = serviceProvider.GetRequiredService<RequestDocumentStore>();
                return new CapturingDocumentAdapter(documentAdapter, requestDocumentStore);
            });
        });
    }

    [Fact]
    public async Task Accepts_top_level_meta_in_patch_resource_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.SupportTickets.Add(existingTicket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                id = existingTicket.StringId,
                attributes = new
                {
                    description = existingTicket.Description
                }
            },
            meta = GetExampleMetaData()
        };

        string route = $"/supportTickets/{existingTicket.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_top_level_meta_in_post_resource_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                attributes = new
                {
                    description = existingTicket.Description
                }
            },
            meta = GetExampleMetaData()
        };

        string route = $"/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        store.Document.Should().NotBeNull();

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_top_level_meta_in_patch_relationship_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        ProductFamily existingProductFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingProductFamily);
            dbContext.SupportTickets.Add(existingTicket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                id = existingTicket.StringId,
                attributes = new
                {
                    description = existingTicket.Description
                },
                relationships = new
                {
                    productFamily = new
                    {
                        data = new
                        {
                            type = "productFamilies",
                            id = existingProductFamily.StringId
                        }
                    }
                }
            },
            meta = GetExampleMetaData()
        };

        string route = $"/supportTickets/{existingTicket.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Data.SingleValue.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().HaveCount(1);

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_top_level_meta_in_post_relationship_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        ProductFamily existingProductFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingProductFamily);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                attributes = new
                {
                    description = existingTicket.Description,
                },
                relationships = new
                {
                    productFamily = new
                    {
                        data = new
                        {
                            type = "productFamilies",
                            id = existingProductFamily.StringId
                        }
                    }
                }
            },
            meta = GetExampleMetaData()
        };

        string route = $"/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        store.Document.Should().NotBeNull();
        store.Document.Data.SingleValue.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().HaveCount(1);

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_top_level_meta_in_delete_relationship_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        ProductFamily existingProductFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            existingTicket.ProductFamily = existingProductFamily;
            dbContext.SupportTickets.Add(existingTicket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null,
            meta = GetExampleMetaData()
        };

        string route = $"/supportTickets/{existingTicket.StringId}/relationships/productFamily";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();
        store.Document.Data.SingleValue.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            var supportTicketInDatabase = await dbContext.SupportTickets
                .Include(supportTicket => supportTicket.ProductFamily)
                .FirstAsync(supportTicket => supportTicket.Id == existingTicket.Id);

            supportTicketInDatabase.ProductFamily.Should().BeNull();
        });

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_top_level_meta_in_atomic_update_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.SupportTickets.Add(existingTicket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "supportTickets",
                        id = existingTicket.StringId,
                        attributes = new
                        {
                            description = existingTicket.Description
                        }
                    }
                }
            },
            meta = GetExampleMetaData()
        };

        string route = $"/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_top_level_meta_in_atomic_add_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "supportTickets",
                        attributes = new
                        {
                            description = existingTicket.Description
                        }
                    }
                }
            },
            meta = GetExampleMetaData()
        };

        string route = $"/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.Document.Should().NotBeNull();
        store.Document.Meta.Should().NotBeNull();

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_top_level_meta_in_atomic_remove_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.SupportTickets.Add(existingTicket);
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
                        type = "supportTickets",
                        id = existingTicket.StringId
                    }
                }
            },
            meta = GetExampleMetaData()
        };

        string route = $"/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        store.Document.Should().NotBeNull();
        store.Document.Meta.Should().NotBeNull();

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_meta_in_data_of_post_resource_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                attributes = new
                {
                    description = existingTicket.Description
                },
                meta = GetExampleMetaData()
            }
        };

        string route = $"/supportTickets/{existingTicket.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        store.Document.Should().NotBeNull();
        store.Document.Data.Should().NotBeNull();
        store.Document.Data.SingleValue.Should().NotBeNull();

        ValidateMetaData(store.Document.Data.SingleValue.Meta);
    }

    [Fact]
    public async Task Accepts_meta_in_relationship_of_post_resource_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        ProductFamily existingProductFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingProductFamily);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                attributes = new
                {
                    description = existingTicket.Description,
                },
                relationships = new
                {
                    productFamily = new
                    {
                        data = new
                        {
                            type = "productFamilies",
                            id = existingProductFamily.StringId
                        },
                        meta = GetExampleMetaData()
                    }
                }
            }
        };

        string route = $"/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        store.Document.Should().NotBeNull();
        store.Document.Data.SingleValue.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().HaveCount(1);
        store.Document.Data.SingleValue.Relationships.TryGetValue("productFamily", out var relationship).Should().BeTrue();
        relationship!.Meta.Should().NotBeNull();

        ValidateMetaData(relationship.Meta);
    }

    [Fact]
    public async Task Accepts_meta_in_relationship_of_atomic_add_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();
        ProductFamily existingProductFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingProductFamily);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "supportTickets",
                        attributes = new
                        {
                            description = existingTicket.Description
                        },
                        relationships = new
                        {
                            productFamily = new
                            {
                                data = new
                                {
                                    type = "productFamilies",
                                    id = existingProductFamily.StringId
                                },
                                meta = GetExampleMetaData()
                            }
                        }
                    }
                }
            }
        };

        string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.Document.Should().NotBeNull();
        store.Document.Operations.Should().NotBeNull();
        store.Document.Operations.Should().HaveCount(1);

        var operation = store.Document.Operations[0];
        operation.Should().NotBeNull();
        operation.Data.Should().NotBeNull();
        operation.Data.SingleValue.Should().NotBeNull();

        var relationships = operation.Data.SingleValue.Relationships;
        relationships.Should().NotBeNull();
        relationships.Should().ContainKey("productFamily");

        var relationship = relationships["productFamily"];
        relationship.Should().NotBeNull();
        relationship.Meta.Should().NotBeNull();

        ValidateMetaData(relationship.Meta);
    }

    [Fact]
    public async Task Accepts_meta_in_data_of_atomic_add_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "supportTickets",
                        attributes = new
                        {
                            description = existingTicket.Description
                        },
                        meta = GetExampleMetaData()
                    }
                }
            }
        };

        string route = $"/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.Document.Should().NotBeNull();
        store.Document.Operations.Should().NotBeNull();
        store.Document.Operations.Should().HaveCount(1);

        var operation = store.Document.Operations[0];
        operation.Should().NotBeNull();
        operation.Data.Should().NotBeNull();
        operation.Data.SingleValue.Should().NotBeNull();
        operation.Data.SingleValue.Meta.Should().NotBeNull();

        ValidateMetaData(operation.Data.SingleValue.Meta);
    }

    [Fact]
    public async Task Accepts_meta_in_relationship_of_atomic_update_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();
        ProductFamily existingProductFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            existingTicket.ProductFamily = existingProductFamily;
            dbContext.SupportTickets.Add(existingTicket);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "supportTickets",
                        id = existingTicket.StringId,
                        relationships = new
                        {
                            productFamily = new
                            {
                                data = (object?)null
                            }
                        }
                    },
                    meta = GetExampleMetaData()
                }
            }
        };

        string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();
        store.Document.Operations.Should().NotBeNull();
        store.Document.Operations.Should().HaveCount(1);

        var operation = store.Document.Operations[0];
        operation.Should().NotBeNull();
        operation.Meta.Should().NotBeNull();

        ValidateMetaData(operation.Meta);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            var ticketInDatabase = await dbContext.SupportTickets
                .Include(supportTicket => supportTicket.ProductFamily)
                .FirstAsync(supportTicket => supportTicket.Id == existingTicket.Id);

            ticketInDatabase.ProductFamily.Should().BeNull();
        });
    }

    private static Object GetExampleMetaData()
    {
        return new
        {
            category = "bug",
            priority = 1,
            urgent = true,
            components = new[]
            {
                "login",
                "single-sign-on"
            },
            relatedTo = new[]
            {
                new
                {
                    id = 123,
                    link = "https://www.ticket-system.com/bugs/123"
                },
                new
                {
                    id = 789,
                    link = "https://www.ticket-system.com/bugs/789"
                }
            },
            contextInfo = new Dictionary<string, object?>
            {
                ["source"] = "form-submission",
                ["retries"] = 1,
                ["authenticated"] = false
            }
        };
    }

    private static void ValidateMetaData(IDictionary<string, object?>? meta)
    {
        meta.Should().NotBeNull();
        meta.Should().HaveCount(6);

        meta.Should().ContainKey("category").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be("bug");
        });

        meta.Should().ContainKey("priority").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be(1);
        });

        meta.Should().ContainKey("components").WhoseValue.With(value =>
        {
            string innerJson = value.Should().BeOfType<JsonElement>().Subject.ToString();

            innerJson.Should().BeJson("""
            [
              "login",
              "single-sign-on"
            ]
            """);
        });

        meta.Should().ContainKey("relatedTo").WhoseValue.With(value =>
        {
            string innerJson = value.Should().BeOfType<JsonElement>().Subject.ToString();

            innerJson.Should().BeJson("""
            [
              {
                "id": 123,
                "link": "https://www.ticket-system.com/bugs/123"
              },
              {
                "id": 789,
                "link": "https://www.ticket-system.com/bugs/789"
              }
            ]
            """);
        });

        meta.Should().ContainKey("contextInfo").WhoseValue.With(value =>
        {
            string innerJson = value.Should().BeOfType<JsonElement>().Subject.ToString();

            innerJson.Should().BeJson("""
            {
              "source": "form-submission",
              "retries": 1,
              "authenticated": false
            }
            """);
        });
    }

    private sealed class CapturingDocumentAdapter : IDocumentAdapter
    {
        private readonly IDocumentAdapter _innerAdapter;
        private readonly RequestDocumentStore _requestDocumentStore;

        public CapturingDocumentAdapter(IDocumentAdapter innerAdapter, RequestDocumentStore requestDocumentStore)
        {
            ArgumentNullException.ThrowIfNull(innerAdapter);
            ArgumentNullException.ThrowIfNull(requestDocumentStore);

            _innerAdapter = innerAdapter;
            _requestDocumentStore = requestDocumentStore;
        }

        public object? Convert(Document document)
        {
            _requestDocumentStore.Document = document;
            return _innerAdapter.Convert(document);
        }
    }

    private sealed class RequestDocumentStore
    {
        public Document? Document { get; set; }
    }
}
