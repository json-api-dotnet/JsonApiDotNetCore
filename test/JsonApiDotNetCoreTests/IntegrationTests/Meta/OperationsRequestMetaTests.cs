using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request.Adapters;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

public sealed class OperationsRequestMetaTests : IClassFixture<IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> _testContext;
    private readonly MetaFakers _fakers = new();

    public OperationsRequestMetaTests(IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> testContext)
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
    public async Task Accepts_meta_in_atomic_update_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();

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
                        },
                        meta = resourceMeta
                    }
                }
            },
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().HaveCount(1);

        AtomicOperationObject? operation = store.Document.Operations[0];
        operation.Should().NotBeNull();
        operation.Data.Should().NotBeNull();

        ResourceObject? resource = operation.Data.SingleValue;
        resource.Should().NotBeNull();
        resource.Meta.Should().BeEquivalentToJson(resourceMeta);
    }

    [Fact]
    public async Task Accepts_meta_in_atomic_remove_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();

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
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);
    }

    [Fact]
    public async Task Accepts_meta_in_relationship_of_atomic_add_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta = _fakers.RelationshipIdentifierMeta.GenerateOne();

        string newTicketDescription = _fakers.SupportTicket.GenerateOne().Description;
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
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
                            description = newTicketDescription
                        },
                        relationships = new
                        {
                            productFamily = new
                            {
                                data = new
                                {
                                    type = "productFamilies",
                                    id = existingFamily.StringId,
                                    meta = identifierMeta
                                },
                                meta = relationshipMeta
                            }
                        },
                        meta = resourceMeta
                    }
                }
            },
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().NotBeNull();
        store.Document.Operations.Should().HaveCount(1);

        AtomicOperationObject? operation = store.Document.Operations[0];
        operation.Should().NotBeNull();

        operation.Data.Should().NotBeNull();
        operation.Data.SingleValue.Should().NotBeNull();

        operation.Data.SingleValue.Meta.Should().BeEquivalentToJson(resourceMeta);

        operation.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();

            value.Meta.Should().BeEquivalentToJson(relationshipMeta);

            value.Data.Should().NotBeNull();

            value.Data.SingleValue.Should().NotBeNull();

            value.Data.SingleValue.Meta.Should().BeEquivalentToJson(identifierMeta);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_relationship_of_atomic_update_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            existingTicket.ProductFamily = existingFamily;
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
                                data = (object?)null,
                                meta = relationshipMeta
                            }
                        }
                    },
                    meta = resourceMeta
                }
            },
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().NotBeNull();
        store.Document.Operations.Should().HaveCount(1);

        AtomicOperationObject? operation = store.Document.Operations[0];
        operation.Should().NotBeNull();

        operation.Meta.Should().BeEquivalentToJson(resourceMeta);

        operation.Data.Should().NotBeNull();

        operation.Data.SingleValue.Should().NotBeNull();

        operation.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Meta.Should().BeEquivalentToJson(relationshipMeta);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_to_one_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContex =>
        {
            dbContex.ProductFamilies.Add(existingFamily);
            dbContex.SupportTickets.Add(existingTicket);
            await dbContex.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "supportTickets",
                        id = existingTicket.StringId,
                        relationship = "productFamily"
                    },
                    data = new
                    {
                        type = "productFamilies",
                        id = existingFamily.StringId,
                        meta = relationshipMeta
                    },
                    meta = resourceMeta
                }
            },
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().NotBeNull();

        AtomicOperationObject? op = store.Document.Operations[0];
        op.Should().NotBeNull();

        op.Meta.Should().BeEquivalentToJson(resourceMeta);

        op.Data.SingleValue.Should().NotBeNull();
        op.Data.SingleValue.Meta.Should().BeEquivalentToJson(relationshipMeta);
    }

    [Fact]
    public async Task Accepts_meta_in_update_to_many_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.RelationshipIdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.RelationshipIdentifierMeta.GenerateOne();

        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();
        SupportTicket existingTicket1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket existingTicket2 = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
            dbContext.SupportTickets.AddRange(existingTicket1, existingTicket2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "productFamilies",
                        id = existingFamily.StringId,
                        relationship = "tickets"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "supportTickets",
                            id = existingTicket1.StringId,
                            meta = identifierMeta1
                        },
                        new
                        {
                            type = "supportTickets",
                            id = existingTicket2.StringId,
                            meta = identifierMeta2
                        }
                    },
                    meta = relationshipMeta
                }
            },
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().NotBeNull();

        AtomicOperationObject? op = store.Document.Operations[0];
        op.Should().NotBeNull();

        op.Meta.Should().BeEquivalentToJson(relationshipMeta);

        op.Data.ManyValue.Should().NotBeNull();
        op.Data.ManyValue.Should().HaveCount(2);

        op.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);

        op.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
    }

    [Fact]
    public async Task Accepts_meta_in_add_to_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.RelationshipIdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.RelationshipIdentifierMeta.GenerateOne();

        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();
        SupportTicket existingTicket1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket existingTicket2 = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
            dbContext.SupportTickets.AddRange(existingTicket1, existingTicket2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    @ref = new
                    {
                        type = "productFamilies",
                        id = existingFamily.StringId,
                        relationship = "tickets"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "supportTickets",
                            id = existingTicket1.StringId,
                            meta = identifierMeta1
                        },
                        new
                        {
                            type = "supportTickets",
                            id = existingTicket2.StringId,
                            meta = identifierMeta2
                        }
                    },
                    meta = relationshipMeta
                }
            },
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().NotBeNull();

        AtomicOperationObject? op = store.Document.Operations[0];
        op.Should().NotBeNull();

        op.Meta.Should().BeEquivalentToJson(relationshipMeta);

        op.Data.ManyValue.Should().NotBeNull();
        op.Data.ManyValue.Should().HaveCount(2);

        op.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);

        op.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
    }

    [Fact]
    public async Task Accepts_meta_in_remove_from_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta = _fakers.RelationshipIdentifierMeta.GenerateOne();

        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();
        SupportTicket existingTicket1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket existingTicket2 = _fakers.SupportTicket.GenerateOne();

        existingFamily.Tickets = new List<SupportTicket>
        {
            existingTicket1,
            existingTicket2
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
            dbContext.SupportTickets.AddRange(existingTicket1, existingTicket2);
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
                        type = "productFamilies",
                        id = existingFamily.StringId,
                        relationship = "tickets"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "supportTickets",
                            id = existingTicket1.StringId,
                            meta = identifierMeta
                        }
                    },
                    meta = relationshipMeta
                }
            },
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().NotBeNull();

        AtomicOperationObject? op = store.Document.Operations[0];
        op.Should().NotBeNull();

        op.Meta.Should().BeEquivalentToJson(relationshipMeta);

        op.Data.ManyValue.Should().NotBeNull();
        op.Data.ManyValue.Should().HaveCount(1);

        op.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta);
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
