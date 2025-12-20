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
    public async Task Accepts_meta_in_patch_resource_request_with_to_one_relationship()
    {
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket ticket = _fakers.SupportTicket.GenerateOne();
        ProductFamily family = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            db.ProductFamilies.Add(family);
            db.SupportTickets.Add(ticket);
            await db.SaveChangesAsync();
        });

        var body = new
        {
            data = new
            {
                type = "supportTickets",
                id = ticket.StringId,
                relationships = new
                {
                    productFamily = new
                    {
                        data = new
                        {
                            type = "productFamilies",
                            id = family.StringId
                        },
                        meta = GetExampleMetaData()
                    }
                },
                meta = GetExampleMetaData()
            },
            meta = GetExampleMetaData()
        };

        (HttpResponseMessage response, _) =
            await _testContext.ExecutePatchAsync<Document>(
                $"/supportTickets/{ticket.StringId}", body);

        response.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        ValidateMetaData(store.Document!.Meta);
        ValidateMetaData(store.Document.Data.SingleValue!.Meta);
    }

    [Fact]
    public async Task Accepts_meta_in_patch_resource_request_with_to_many_relationship()
    {
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        ProductFamily family = _fakers.ProductFamily.GenerateOne();
        SupportTicket t1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket t2 = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            db.ProductFamilies.Add(family);
            db.SupportTickets.AddRange(t1, t2);
            await db.SaveChangesAsync();
        });

        var body = new
        {
            data = new
            {
                type = "productFamilies",
                id = family.StringId,
                relationships = new
                {
                    tickets = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "supportTickets",
                                id = t1.StringId,
                                meta = GetExampleMetaData()
                            },
                            new
                            {
                                type = "supportTickets",
                                id = t2.StringId,
                                meta = GetExampleMetaData()
                            }
                        },
                        meta = GetExampleMetaData()
                    }
                },
                meta = GetExampleMetaData()
            },
            meta = GetExampleMetaData()
        };

        (HttpResponseMessage response, _) =
            await _testContext.ExecutePatchAsync<Document>(
                $"/productFamilies/{family.StringId}", body);

        response.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        ValidateMetaData(store.Document!.Meta);
    }

    [Fact]
    public async Task Accepts_meta_in_post_resource_request_with_relationship()
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

        const string route = "/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        store.Document.Should().NotBeNull();
        store.Document.Data.SingleValue.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().NotBeNull();
        store.Document.Data.SingleValue.Relationships.Should().HaveCount(1);

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_meta_in_delete_relationship_request()
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
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();
        store.Document.Data.SingleValue.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            SupportTicket supportTicketInDatabase = await dbContext.SupportTickets.Include(supportTicket => supportTicket.ProductFamily)
                .FirstAsync(supportTicket => supportTicket.Id == existingTicket.Id);

            supportTicketInDatabase.ProductFamily.Should().BeNull();
        });

        ValidateMetaData(store.Document.Meta);
    }

    [Fact]
    public async Task Accepts_meta_in_atomic_update_resource_operation()
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

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        ValidateMetaData(store.Document.Meta);

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            SupportTicket updated = await db.SupportTickets.FirstAsync(t => t.Id == existingTicket.Id);
            updated.Description.Should().Be(existingTicket.Description);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_atomic_remove_resource_operation()
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

        const string route = "/operations";

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
            },
            meta = GetExampleMetaData()
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        store.Document.Should().NotBeNull();
        store.Document.Operations.Should().NotBeNull();
        store.Document.Operations.Should().HaveCount(1);

        AtomicOperationObject? operation = store.Document.Operations[0];
        operation.Should().NotBeNull();
        operation.Data.Should().NotBeNull();
        operation.Data.SingleValue.Should().NotBeNull();

        IDictionary<string, RelationshipObject?>? relationships = operation.Data.SingleValue.Relationships;
        relationships.Should().NotBeNull();
        relationships.Should().ContainKey("productFamily");

        RelationshipObject? relationship = relationships["productFamily"];
        relationship.Should().NotBeNull();
        relationship.Meta.Should().NotBeNull();

        ValidateMetaData(relationship.Meta);

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            SupportTicket ticket = await db.SupportTickets
                .Include(t => t.ProductFamily)
                .FirstAsync();

            ticket.ProductFamily.Should().NotBeNull();
        });
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

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();
        store.Document.Operations.Should().NotBeNull();
        store.Document.Operations.Should().HaveCount(1);

        AtomicOperationObject? operation = store.Document.Operations[0];
        operation.Should().NotBeNull();
        operation.Meta.Should().NotBeNull();

        ValidateMetaData(operation.Meta);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            SupportTicket supportTicketInDatabase = await dbContext.SupportTickets.Include(supportTicket => supportTicket.ProductFamily)
                .FirstAsync(supportTicket => supportTicket.Id == existingTicket.Id);

            supportTicketInDatabase.ProductFamily.Should().BeNull();
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_to_one_relationship_operation()
    {
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        SupportTicket ticket = _fakers.SupportTicket.GenerateOne();
        ProductFamily family = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            db.ProductFamilies.Add(family);
            db.SupportTickets.Add(ticket);
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
                        id = ticket.StringId,
                        relationship = "productFamily"
                    },
                    data = new
                    {
                        type = "productFamilies",
                        id = family.StringId,
                        meta = GetExampleMetaData()
                    },
                    meta = GetExampleMetaData()
                }
            },
            meta = GetExampleMetaData()
        };

        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        store.Document.Should().NotBeNull();
        ValidateMetaData(store.Document.Meta);
        AtomicOperationObject? op = store.Document.Operations![0];
        op.Should().NotBeNull();
        ValidateMetaData(op.Meta);
        op.Data.SingleValue.Should().NotBeNull();
        ValidateMetaData(op.Data.SingleValue.Meta);

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            SupportTicket dbTicket = await db.SupportTickets.Include(t => t.ProductFamily).FirstAsync(t => t.Id == ticket.Id);
            dbTicket.ProductFamily!.Id.Should().Be(family.Id);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_to_many_relationship_operation()
    {
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        ProductFamily family = _fakers.ProductFamily.GenerateOne();
        SupportTicket ticket1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket ticket2 = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            db.ProductFamilies.Add(family);
            db.SupportTickets.AddRange(ticket1, ticket2);
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
                        type = "productFamilies",
                        id = family.StringId,
                        relationship = "tickets"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "supportTickets",
                            id = ticket1.StringId,
                            meta = GetExampleMetaData()
                        },
                        new
                        {
                            type = "supportTickets",
                            id = ticket2.StringId,
                            meta = GetExampleMetaData()
                        }
                    },
                    meta = GetExampleMetaData()
                }
            },
            meta = GetExampleMetaData()
        };

        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        store.Document.Should().NotBeNull();
        ValidateMetaData(store.Document.Meta);
        AtomicOperationObject? op = store.Document.Operations![0];
        op.Should().NotBeNull();
        ValidateMetaData(op.Meta);

        foreach (ResourceObject data in op.Data.ManyValue!)
        {
            ValidateMetaData(data.Meta);
        }

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            ProductFamily dbFamily = await db.ProductFamilies.Include(f => f.Tickets).FirstAsync(f => f.Id == family.Id);
            dbFamily.Tickets.Should().ContainSingle(t => t.Id == ticket1.Id);
            dbFamily.Tickets.Should().ContainSingle(t => t.Id == ticket2.Id);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_add_to_relationship_operation()
    {
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        ProductFamily family = _fakers.ProductFamily.GenerateOne();
        SupportTicket ticket1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket ticket2 = _fakers.SupportTicket.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            db.ProductFamilies.Add(family);
            db.SupportTickets.AddRange(ticket1, ticket2);
            await db.SaveChangesAsync();
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
                        id = family.StringId,
                        relationship = "tickets"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "supportTickets",
                            id = ticket1.StringId,
                            meta = GetExampleMetaData()
                        },
                        new
                        {
                            type = "supportTickets",
                            id = ticket2.StringId,
                            meta = GetExampleMetaData()
                        }
                    },
                    meta = GetExampleMetaData()
                }
            },
            meta = GetExampleMetaData()
        };

        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        store.Document.Should().NotBeNull();
        ValidateMetaData(store.Document.Meta);
        AtomicOperationObject? op = store.Document.Operations![0];
        op.Should().NotBeNull();
        ValidateMetaData(op.Meta);

        foreach (ResourceObject data in op.Data.ManyValue!)
        {
            ValidateMetaData(data.Meta);
        }

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            ProductFamily dbFamily = await db.ProductFamilies.Include(f => f.Tickets).FirstAsync(f => f.Id == family.Id);
            dbFamily.Tickets.Should().ContainSingle(t => t.Id == ticket1.Id);
            dbFamily.Tickets.Should().ContainSingle(t => t.Id == ticket2.Id);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_delete_from_relationship_operation()
    {
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        ProductFamily family = _fakers.ProductFamily.GenerateOne();
        SupportTicket ticket1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket ticket2 = _fakers.SupportTicket.GenerateOne();

        family.Tickets = new List<SupportTicket>
        {
            ticket1,
            ticket2
        };

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            db.ProductFamilies.Add(family);
            db.SupportTickets.AddRange(ticket1, ticket2);
            await db.SaveChangesAsync();
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
                        id = family.StringId,
                        relationship = "tickets"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "supportTickets",
                            id = ticket1.StringId,
                            meta = GetExampleMetaData()
                        }
                    },
                    meta = GetExampleMetaData()
                }
            },
            meta = GetExampleMetaData()
        };

        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        store.Document.Should().NotBeNull();
        ValidateMetaData(store.Document.Meta);
        AtomicOperationObject? op = store.Document.Operations![0];
        op.Should().NotBeNull();
        ValidateMetaData(op.Meta);

        foreach (ResourceObject data in op.Data.ManyValue!)
        {
            ValidateMetaData(data.Meta);
        }

        await _testContext.RunOnDatabaseAsync(async db =>
        {
            ProductFamily dbFamily = await db.ProductFamilies.Include(f => f.Tickets).FirstAsync(f => f.Id == family.Id);
            dbFamily.Tickets.Should().NotContain(t => t.Id == ticket1.Id);
            dbFamily.Tickets.Should().ContainSingle(t => t.Id == ticket2.Id);
        });
    }

    private static object GetExampleMetaData()
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
