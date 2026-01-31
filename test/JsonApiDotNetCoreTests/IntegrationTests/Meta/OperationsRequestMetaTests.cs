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
    public async Task Accepts_meta_in_update_resource_operation_with_ToOne_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> operationMeta = _fakers.OperationMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta = _fakers.IdentifierMeta.GenerateOne();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();
        string newTicketDescription = _fakers.SupportTicket.GenerateOne().Description;
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
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
                    },
                    meta = operationMeta
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

        store.Document.Operations.Should().ContainSingle().Which.With(operation =>
        {
            operation.Should().NotBeNull();
            operation.Meta.Should().BeEquivalentToJson(operationMeta);
            operation.Data.SingleValue.Should().NotBeNull();
            operation.Data.SingleValue.Meta.Should().BeEquivalentToJson(resourceMeta);

            operation.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Meta.Should().BeEquivalentToJson(relationshipMeta);
                value.Data.SingleValue.Should().NotBeNull();
                value.Data.SingleValue.Meta.Should().BeEquivalentToJson(identifierMeta);
            });
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_resource_operation_with_ToMany_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> operationMeta = _fakers.OperationMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.IdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.IdentifierMeta.GenerateOne();

        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();
        string newFamilyName = _fakers.ProductFamily.GenerateOne().Name;
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
                    data = new
                    {
                        type = "productFamilies",
                        id = existingFamily.StringId,
                        attributes = new
                        {
                            name = newFamilyName
                        },
                        relationships = new
                        {
                            tickets = new
                            {
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
                        meta = resourceMeta
                    },
                    meta = operationMeta
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

        store.Document.Operations.Should().ContainSingle().Which.With(operation =>
        {
            operation.Should().NotBeNull();
            operation.Meta.Should().BeEquivalentToJson(operationMeta);
            operation.Data.SingleValue.Should().NotBeNull();
            operation.Data.SingleValue.Meta.Should().BeEquivalentToJson(resourceMeta);

            operation.Data.SingleValue.Relationships.Should().ContainKey("tickets").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Meta.Should().BeEquivalentToJson(relationshipMeta);
                value.Data.ManyValue.Should().HaveCount(2);
                value.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);
                value.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
            });
        });
    }

    [Fact]
    public async Task Accepts_meta_in_add_resource_operation_with_ToOne_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> operationMeta = _fakers.OperationMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta = _fakers.IdentifierMeta.GenerateOne();

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
                    },
                    meta = operationMeta
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
        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().ContainSingle().Which.With(operation =>
        {
            operation.Should().NotBeNull();
            operation.Meta.Should().BeEquivalentToJson(operationMeta);
            operation.Data.SingleValue.Should().NotBeNull();
            operation.Data.SingleValue.Meta.Should().BeEquivalentToJson(resourceMeta);

            operation.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Meta.Should().BeEquivalentToJson(relationshipMeta);
                value.Data.SingleValue.Should().NotBeNull();
                value.Data.SingleValue.Meta.Should().BeEquivalentToJson(identifierMeta);
            });
        });
    }

    [Fact]
    public async Task Accepts_meta_in_add_resource_operation_with_ToMany_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> operationMeta = _fakers.OperationMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.IdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.IdentifierMeta.GenerateOne();

        string newFamilyName = _fakers.ProductFamily.GenerateOne().Name;
        SupportTicket existingTicket1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket existingTicket2 = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
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
                    data = new
                    {
                        type = "productFamilies",
                        attributes = new
                        {
                            name = newFamilyName
                        },
                        relationships = new
                        {
                            tickets = new
                            {
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
                        meta = resourceMeta
                    },
                    meta = operationMeta
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
        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().ContainSingle().Which.With(operation =>
        {
            operation.Should().NotBeNull();
            operation.Meta.Should().BeEquivalentToJson(operationMeta);
            operation.Data.SingleValue.Should().NotBeNull();
            operation.Data.SingleValue.Meta.Should().BeEquivalentToJson(resourceMeta);

            operation.Data.SingleValue.Relationships.Should().ContainKey("tickets").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Meta.Should().BeEquivalentToJson(relationshipMeta);
                value.Data.ManyValue.Should().HaveCount(2);
                value.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);
                value.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
            });
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_ToOne_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> operationMeta = _fakers.OperationMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta = _fakers.IdentifierMeta.GenerateOne();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
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
                        meta = identifierMeta
                    },
                    meta = operationMeta
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

        store.Document.Operations.Should().ContainSingle().Which.With(operation =>
        {
            operation.Should().NotBeNull();
            operation.Meta.Should().BeEquivalentToJson(operationMeta);
            operation.Data.SingleValue.Should().NotBeNull();
            operation.Data.SingleValue.Meta.Should().BeEquivalentToJson(identifierMeta);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_ToMany_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> operationMeta = _fakers.OperationMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.IdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.IdentifierMeta.GenerateOne();

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
                    meta = operationMeta
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

        store.Document.Operations.Should().ContainSingle().Which.With(operation =>
        {
            operation.Should().NotBeNull();
            operation.Meta.Should().BeEquivalentToJson(operationMeta);
            operation.Data.ManyValue.Should().HaveCount(2);
            operation.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);
            operation.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_add_to_ToMany_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> operationMeta = _fakers.OperationMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.IdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.IdentifierMeta.GenerateOne();

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
                    meta = operationMeta
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

        store.Document.Operations.Should().ContainSingle().Which.With(operation =>
        {
            operation.Should().NotBeNull();
            operation.Meta.Should().BeEquivalentToJson(operationMeta);
            operation.Data.ManyValue.Should().HaveCount(2);
            operation.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);
            operation.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_remove_from_ToMany_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> operationMeta = _fakers.OperationMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.IdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.IdentifierMeta.GenerateOne();

        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();
        existingFamily.Tickets = _fakers.SupportTicket.GenerateList(2);

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
                            id = existingFamily.Tickets[0].StringId,
                            meta = identifierMeta1
                        },
                        new
                        {
                            type = "supportTickets",
                            id = existingFamily.Tickets[1].StringId,
                            meta = identifierMeta2
                        }
                    },
                    meta = operationMeta
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

        store.Document.Operations.Should().ContainSingle().Which.With(operation =>
        {
            operation.Should().NotBeNull();
            operation.Meta.Should().BeEquivalentToJson(operationMeta);
            operation.Data.ManyValue.Should().HaveCount(2);
            operation.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);
            operation.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_remove_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> operationMeta = _fakers.OperationMeta.GenerateOne();

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
                    },
                    meta = operationMeta
                }
            },
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();
        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Operations.Should().ContainSingle().Which.With(operation =>
        {
            operation.Should().NotBeNull();
            operation.Meta.Should().BeEquivalentToJson(operationMeta);
        });
    }
}
