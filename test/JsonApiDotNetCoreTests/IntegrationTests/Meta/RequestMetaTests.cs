using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request.Adapters;
using JsonApiDotNetCore.Serialization.Response;
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
    public async Task Accepts_meta_in_patch_resource_request_with_to_one_relationship()
    {
        // Assert
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();
        var resourceMeta = _fakers.ResourceMeta.Generate();
        var relationshipMeta = _fakers.RelationshipMeta.Generate();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            db.ProductFamilies.Add(existingFamily);
            db.SupportTickets.Add(existingTicket);
            await db.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "supportTickets",
                id = existingTicket.StringId,
                relationships = new
                {
                    productFamily = new
                    {
                        data = new
                        {
                            type = "productFamilies",
                            id = existingFamily.StringId
                        },
                        meta = relationshipMeta
                    }
                },
                meta = resourceMeta
            },
            meta = documentMeta
        };

        string route = $"/supportTickets/{existingTicket.StringId}";

        // Act
        (HttpResponseMessage response, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Data.Should().NotBeNull();
        store.Document.Data.SingleValue.Should().NotBeNull();
        store.Document.Data.SingleValue.Meta.Should().HaveCount(resourceMeta.Count);
        store.Document.Data.SingleValue.Meta.Should().ContainKey("editedBy").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)resourceMeta["editedBy"]);
        });
        store.Document.Data.SingleValue.Meta.Should().ContainKey("revision").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be((int)resourceMeta["revision"]);
        });

        store.Document.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();

            value.Meta.Should().HaveCount(relationshipMeta.Count);
            value.Meta.Should().ContainKey("source").WhoseValue.With(val =>
            {
                JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be((string)relationshipMeta["source"]);
            });
            value.Meta.Should().ContainKey("confidence").WhoseValue.With(val =>
            {
                JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
                element.GetDouble().Should().BeApproximately((double)relationshipMeta["confidence"], 1e-6);
            });
        });
    }

    [Fact]
    public async Task Accepts_meta_in_patch_resource_request_with_to_many_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();
        var resourceMeta = _fakers.ResourceMeta.Generate();
        var relationshipMeta = _fakers.RelationshipMeta.Generate();
        var identifierMeta1 = _fakers.RelationshipIdentifierMeta.Generate();
        var identifierMeta2 = _fakers.RelationshipIdentifierMeta.Generate();

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
            data = new
            {
                type = "productFamilies",
                id = existingFamily.StringId,
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
            meta = documentMeta
        };

        string route = $"/productFamilies/{existingFamily.StringId}";

        // Act
        (HttpResponseMessage response, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        // resource meta explicit validation
        store.Document.Data.SingleValue.Should().NotBeNull();
        store.Document.Data.SingleValue.Meta.Should().HaveCount(resourceMeta.Count);
        store.Document.Data.SingleValue.Meta.Should().ContainKey("editedBy").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)resourceMeta["editedBy"]);
        });
        store.Document.Data.SingleValue.Meta.Should().ContainKey("revision").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be((int)resourceMeta["revision"]);
        });

        // relationship meta validation
        store.Document.Data.SingleValue.Relationships.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().ContainKey("tickets").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Meta.Should().HaveCount(relationshipMeta.Count);
            value.Meta.Should().ContainKey("source").WhoseValue.With(val =>
            {
                JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be((string)relationshipMeta["source"]);
            });
            value.Meta.Should().ContainKey("confidence").WhoseValue.With(val =>
            {
                JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
                element.GetDouble().Should().BeApproximately((double)relationshipMeta["confidence"], 1e-6);
            });

            value.Data.ManyValue.Should().HaveCount(2);

            value.Data.ManyValue[0].Meta.Should().HaveCount(identifierMeta1.Count);
            value.Data.ManyValue[0].Meta.Should().ContainKey("index").WhoseValue.With(v =>
            {
                JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
                element.GetInt32().Should().Be((int)identifierMeta1["index"]);
            });
            value.Data.ManyValue[0].Meta.Should().ContainKey("optionalNote").WhoseValue.With(v =>
            {
                JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be((string)identifierMeta1["optionalNote"]);
            });

            value.Data.ManyValue[1].Meta.Should().HaveCount(identifierMeta2.Count);
            value.Data.ManyValue[1].Meta.Should().ContainKey("index").WhoseValue.With(v =>
            {
                JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
                element.GetInt32().Should().Be((int)identifierMeta2["index"]);
            });
            value.Data.ManyValue[1].Meta.Should().ContainKey("optionalNote").WhoseValue.With(v =>
            {
                JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be((string)identifierMeta2["optionalNote"]);
            });
        });
    }

    [Fact]
    public async Task Accepts_meta_in_post_resource_request_with_to_one_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();

        string newTicketDescription = _fakers.SupportTicket.GenerateOne().Description;
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
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
                            id = existingFamily.StringId
                        }
                    }
                }
            },
            meta = documentMeta
        };

        const string route = "/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        store.Document.Should().NotBeNull();

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Data.SingleValue.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.SingleValue.Should().NotBeNull();
            value.Data.SingleValue.Type.Should().Be("productFamilies");
            value.Data.SingleValue.Id.Should().Be(existingFamily.StringId);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_post_resource_request_with_to_many_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();
        var resourceMeta = _fakers.ResourceMeta.Generate();
        var identifierMeta1 = _fakers.RelationshipIdentifierMeta.Generate();
        var identifierMeta2 = _fakers.RelationshipIdentifierMeta.Generate();

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
                                description = existingTicket1.Description,
                                meta = identifierMeta1
                            },
                            new
                            {
                                type = "supportTickets",
                                id = existingTicket2.StringId,
                                description = existingTicket2.Description,
                                meta = identifierMeta2
                            }
                        },
                        meta = resourceMeta
                    }
                }
            },
            meta = documentMeta
        };

        const string route = "/productFamilies";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        store.Document.Should().NotBeNull();

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Data.SingleValue.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().ContainKey("tickets").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.ManyValue.Should().HaveCount(2);

            value.Data.ManyValue[0].Type.Should().Be("supportTickets");
            value.Data.ManyValue[0].Id.Should().Be(existingTicket1.StringId);

            value.Data.ManyValue[1].Type.Should().Be("supportTickets");
            value.Data.ManyValue[1].Id.Should().Be(existingTicket2.StringId);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_delete_relationship_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();

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
            data = (object?)null,
            meta = documentMeta
        };

        string route = $"/supportTickets/{existingTicket.StringId}/relationships/productFamily";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Data.SingleValue.Should().BeNull();

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_atomic_update_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();

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
            meta = documentMeta
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Operations.Should().HaveCount(1);

        AtomicOperationObject? operation = store.Document.Operations[0];
        operation.Should().NotBeNull();
        operation.Data.Should().NotBeNull();

        ResourceObject? resource = operation.Data.SingleValue;
        resource.Should().NotBeNull();
        resource.Type.Should().Be("supportTickets");
        resource.Id.Should().Be(existingTicket.StringId);
        resource.Attributes.Should().NotBeNull();
        resource.Attributes.Should().ContainKey("description");
    }

    [Fact]
    public async Task Accepts_meta_in_atomic_remove_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();

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

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_relationship_of_atomic_add_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();
        var resourceMeta = _fakers.ResourceMeta.Generate();
        var relationshipMeta = _fakers.RelationshipMeta.Generate();

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
                                    id = existingFamily.StringId
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

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Operations.Should().NotBeNull();
        store.Document.Operations.Should().HaveCount(1);

        AtomicOperationObject? operation = store.Document.Operations[0];
        operation.Should().NotBeNull();

        operation.Data.Should().NotBeNull();
        operation.Data.SingleValue.Should().NotBeNull();

        // resource/meta validation on operation data (resourceMeta -> relationshipMeta mapping)
        operation.Data.SingleValue.Meta.Should().HaveCount(relationshipMeta.Count);
        operation.Data.SingleValue.Meta.Should().ContainKey("editedBy").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)resourceMeta["editedBy"]);
        });
        operation.Data.SingleValue.Meta.Should().ContainKey("revision").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetDouble().Should().Be((int)resourceMeta["revision"]);
        });

        operation.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Meta.Should().NotBeNull();
            value.Meta.Should().HaveCount(relationshipMeta.Count);
            value.Meta.Should().ContainKey("source").WhoseValue.With(v =>
            {
                JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be((string)relationshipMeta["source"]);
            });
            value.Meta.Should().ContainKey("confidence").WhoseValue.With(v =>
            {
                JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
                element.GetDouble().Should().BeApproximately((double)relationshipMeta["confidence"], 1e-6);
            });
        });
    }

    [Fact]
    public async Task Accepts_meta_in_relationship_of_atomic_update_resource_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();
        var resourceMeta = _fakers.ResourceMeta.Generate();
        var relationshipMeta = _fakers.RelationshipMeta.Generate();

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

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Operations.Should().NotBeNull();
        store.Document.Operations.Should().HaveCount(1);

        AtomicOperationObject? operation = store.Document.Operations[0];
        operation.Should().NotBeNull();

        // operation meta explicit validation (resourceMeta)
        operation.Meta.Should().HaveCount(resourceMeta.Count);
        operation.Meta.Should().ContainKey("editedBy").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)resourceMeta["editedBy"]);
        });
        operation.Meta.Should().ContainKey("revision").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be((int)resourceMeta["revision"]);
        });

        operation.Data.Should().NotBeNull();

        operation.Data.SingleValue.Should().NotBeNull();
        operation.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Meta.Should().NotBeNull();

            value.Meta.Should().HaveCount(relationshipMeta.Count);
            value.Meta.Should().ContainKey("source").WhoseValue.With(v =>
            {
                JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
                element.GetString().Should().Be((string)relationshipMeta["source"]);
            });
            value.Meta.Should().ContainKey("confidence").WhoseValue.With(v =>
            {
                JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
                element.GetDouble().Should().BeApproximately((double)relationshipMeta["confidence"], 1e-6);
            });
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_to_one_relationship_operation()
    {
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();
        var resourceMeta = _fakers.ResourceMeta.Generate();
        var relationshipMeta = _fakers.RelationshipMeta.Generate();

        SupportTicket existingTicket = _fakers.SupportTicket.GenerateOne();
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            db.ProductFamilies.Add(existingFamily);
            db.SupportTickets.Add(existingTicket);
            await db.SaveChangesAsync();
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

        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Operations.Should().NotBeNull();

        AtomicOperationObject? op = store.Document.Operations[0];
        op.Should().NotBeNull();

        // operation meta explicit validation (resourceMeta)
        op.Meta.Should().HaveCount(resourceMeta.Count);
        op.Meta.Should().ContainKey("editedBy").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)resourceMeta["editedBy"]);
        });
        op.Meta.Should().ContainKey("revision").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be((int)resourceMeta["revision"]);
        });

        op.Data.SingleValue.Should().NotBeNull();
        op.Data.SingleValue.Meta.Should().NotBeNull();

        // data meta explicit validation (relationshipMeta)
        op.Data.SingleValue.Meta.Should().HaveCount(relationshipMeta.Count);
        op.Data.SingleValue.Meta.Should().ContainKey("source").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)relationshipMeta["source"]);
        });
        op.Data.SingleValue.Meta.Should().ContainKey("confidence").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetDouble().Should().BeApproximately((double)relationshipMeta["confidence"], 1e-6);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_to_many_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();
        var relationshipMeta = _fakers.RelationshipMeta.Generate();
        var identifierMeta1 = _fakers.RelationshipIdentifierMeta.Generate();
        var identifierMeta2 = _fakers.RelationshipIdentifierMeta.Generate();

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

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Operations.Should().NotBeNull();

        AtomicOperationObject? op = store.Document.Operations[0];
        op.Should().NotBeNull();

        // op meta explicit validation (relationshipMeta)
        op.Meta.Should().HaveCount(relationshipMeta.Count);
        op.Meta.Should().ContainKey("source").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)relationshipMeta["source"]);
        });
        op.Meta.Should().ContainKey("confidence").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetDouble().Should().BeApproximately((double)relationshipMeta["confidence"], 1e-6);
        });

        op.Data.ManyValue.Should().NotBeNull();
        op.Data.ManyValue.Should().HaveCount(2);

        op.Data.ManyValue[0].Meta.Should().HaveCount(identifierMeta1.Count);
        op.Data.ManyValue[0].Meta.Should().ContainKey("index").WhoseValue.With(v =>
        {
            JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be((int)identifierMeta1["index"]);
        });
        op.Data.ManyValue[0].Meta.Should().ContainKey("optionalNote").WhoseValue.With(v =>
        {
            JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)identifierMeta1["optionalNote"]);
        });

        op.Data.ManyValue[1].Meta.Should().HaveCount(identifierMeta2.Count);
        op.Data.ManyValue[1].Meta.Should().ContainKey("index").WhoseValue.With(v =>
        {
            JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be((int)identifierMeta2["index"]);
        });
        op.Data.ManyValue[1].Meta.Should().ContainKey("optionalNote").WhoseValue.With(v =>
        {
            JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)identifierMeta2["optionalNote"]);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_add_to_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();
        var relationshipMeta = _fakers.RelationshipMeta.Generate();
        var identifierMeta1 = _fakers.RelationshipIdentifierMeta.Generate();
        var identifierMeta2 = _fakers.RelationshipIdentifierMeta.Generate();

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

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Operations.Should().NotBeNull();

        AtomicOperationObject? op = store.Document.Operations[0];
        op.Should().NotBeNull();

        // op meta explicit validation (relationshipMeta)
        op.Meta.Should().HaveCount(relationshipMeta.Count);
        op.Meta.Should().ContainKey("source").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)relationshipMeta["source"]);
        });
        op.Meta.Should().ContainKey("confidence").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetDouble().Should().BeApproximately((double)relationshipMeta["confidence"], 1e-6);
        });

        op.Data.ManyValue.Should().NotBeNull();
        op.Data.ManyValue.Should().HaveCount(2);

        op.Data.ManyValue[0].Meta.Should().HaveCount(identifierMeta1.Count);
        op.Data.ManyValue[0].Meta.Should().ContainKey("index").WhoseValue.With(v =>
        {
            JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be((int)identifierMeta1["index"]);
        });
        op.Data.ManyValue[0].Meta.Should().ContainKey("optionalNote").WhoseValue.With(v =>
        {
            JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)identifierMeta1["optionalNote"]);
        });

        op.Data.ManyValue[1].Meta.Should().HaveCount(identifierMeta2.Count);
        op.Data.ManyValue[1].Meta.Should().ContainKey("index").WhoseValue.With(v =>
        {
            JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be((int)identifierMeta2["index"]);
        });
        op.Data.ManyValue[1].Meta.Should().ContainKey("optionalNote").WhoseValue.With(v =>
        {
            JsonElement element = v.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)identifierMeta2["optionalNote"]);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_delete_from_relationship_operation()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        var documentMeta = _fakers.DocumentMeta.Generate();
        var relationshipMeta = _fakers.RelationshipMeta.Generate();

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

        // document meta explicit validation
        store.Document.Meta.Should().HaveCount(documentMeta.Count);
        store.Document.Meta.Should().ContainKey("requestId").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)documentMeta["requestId"]);
        });

        store.Document.Operations.Should().NotBeNull();

        AtomicOperationObject? op = store.Document.Operations[0];
        op.Should().NotBeNull();

        // op meta explicit validation (relationshipMeta)
        op.Meta.Should().HaveCount(relationshipMeta.Count);
        op.Meta.Should().ContainKey("source").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be((string)relationshipMeta["source"]);
        });
        op.Meta.Should().ContainKey("confidence").WhoseValue.With(val =>
        {
            JsonElement element = val.Should().BeOfType<JsonElement>().Subject;
            element.GetDouble().Should().BeApproximately((double)relationshipMeta["confidence"], 1e-6);
        });

        op.Data.ManyValue.Should().NotBeNull();
        op.Data.ManyValue.Should().HaveCount(1);
        op.Data.ManyValue[0].Type.Should().Be("supportTickets");
        op.Data.ManyValue[0].Id.Should().Be(existingTicket1.StringId);
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
