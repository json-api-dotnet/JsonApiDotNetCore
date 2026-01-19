using System.Net;
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
    public async Task Accepts_meta_in_update_resource_request_with_ToOne_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
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
                            id = existingFamily.StringId,
                            meta = identifierMeta
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

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Data.SingleValue.Should().NotBeNull();

        store.Document.Data.SingleValue.Meta.Should().BeEquivalentToJson(resourceMeta);

        store.Document.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Meta.Should().BeEquivalentToJson(relationshipMeta);
            value.Data.SingleValue.Should().NotBeNull();
            value.Data.SingleValue.Meta.Should().BeEquivalentToJson(identifierMeta);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_resource_request_with_ToMany_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> resourceMeta = _fakers.ResourceMeta.GenerateOne();
        Dictionary<string, object?> relationshipMeta = _fakers.RelationshipMeta.GenerateOne();
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

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Data.SingleValue.Should().NotBeNull();

        store.Document.Data.SingleValue.Meta.Should().BeEquivalentToJson(resourceMeta);

        store.Document.Data.SingleValue.Relationships.Should().NotBeNull();

        store.Document.Data.SingleValue.Relationships.Should().ContainKey("tickets").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Meta.Should().BeEquivalentToJson(relationshipMeta);

            value.Data.ManyValue.Should().HaveCount(2);

            value.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);
            value.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_add_resource_request_with_ToOne_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
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
            meta = documentMeta
        };

        const string route = "/supportTickets";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);
        store.Document.Data.SingleValue.Should().NotBeNull();

        store.Document.Data.SingleValue.Meta.Should().BeEquivalentToJson(resourceMeta);

        store.Document.Data.SingleValue.Relationships.Should().NotBeNull();

        store.Document.Data.SingleValue.Relationships.Should().ContainKey("productFamily").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();

            value.Meta.Should().BeEquivalentToJson(relationshipMeta);

            value.Data.SingleValue.Should().NotBeNull();

            value.Data.SingleValue.Meta.Should().BeEquivalentToJson(identifierMeta);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_add_resource_request_with_ToMany_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
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
            meta = documentMeta
        };

        const string route = "/productFamilies";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Data.SingleValue.Should().NotBeNull();
        store.Document.Data.SingleValue.Meta.Should().BeEquivalentToJson(resourceMeta);

        store.Document.Data.SingleValue.Relationships.Should().NotBeNull();

        store.Document.Data.SingleValue.Relationships.Should().ContainKey("tickets").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();

            value.Meta.Should().BeEquivalentToJson(relationshipMeta);

            value.Data.ManyValue.Should().HaveCount(2);

            value.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);

            value.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
        });
    }

    [Fact]
    public async Task Accepts_meta_in_update_ToOne_relationship_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
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
            data = new
            {
                type = "productFamilies",
                id = existingFamily.StringId,
                meta = identifierMeta
            },
            meta = documentMeta
        };

        string route = $"/supportTickets/{existingTicket.StringId}/relationships/productFamily";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Data.SingleValue.Should().NotBeNull();

        store.Document.Data.SingleValue.Meta.Should().BeEquivalentToJson(identifierMeta);
    }

    [Fact]
    public async Task Accepts_meta_in_update_ToMany_relationship_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.IdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.IdentifierMeta.GenerateOne();

        SupportTicket existingTicket1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket existingTicket2 = _fakers.SupportTicket.GenerateOne();
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
            dbContext.SupportTickets.AddRange(existingTicket1, existingTicket2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
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
            meta = documentMeta
        };

        string route = $"/productFamilies/{existingFamily.StringId}/relationships/tickets";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Data.ManyValue.Should().HaveCount(2);

        store.Document.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);

        store.Document.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
    }

    [Fact]
    public async Task Accepts_meta_in_add_ToMany_relationship_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.IdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.IdentifierMeta.GenerateOne();

        SupportTicket existingTicket1 = _fakers.SupportTicket.GenerateOne();
        SupportTicket existingTicket2 = _fakers.SupportTicket.GenerateOne();
        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
            dbContext.SupportTickets.AddRange(existingTicket1, existingTicket2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
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
            meta = documentMeta
        };

        string route = $"/productFamilies/{existingFamily.StringId}/relationships/tickets";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Data.ManyValue.Should().HaveCount(2);

        store.Document.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);

        store.Document.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
    }

    [Fact]
    public async Task Accepts_meta_in_remove_from_ToMany_relationship_request()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<RequestDocumentStore>();

        Dictionary<string, object?> documentMeta = _fakers.DocumentMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta1 = _fakers.IdentifierMeta.GenerateOne();
        Dictionary<string, object?> identifierMeta2 = _fakers.IdentifierMeta.GenerateOne();

        ProductFamily existingFamily = _fakers.ProductFamily.GenerateOne();
        SupportTicket existingTicket1 = _fakers.SupportTicket.GenerateOne();
        existingTicket1.ProductFamily = existingFamily;
        SupportTicket existingTicket2 = _fakers.SupportTicket.GenerateOne();
        existingTicket2.ProductFamily = existingFamily;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ProductFamilies.Add(existingFamily);
            dbContext.SupportTickets.AddRange(existingTicket1, existingTicket2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "supportTickets",
                    id = existingTicket2.StringId,
                    meta = identifierMeta1
                },
                new
                {
                    type = "supportTickets",
                    id = existingTicket2.StringId,
                    meta = identifierMeta2
                }
            },
            meta = documentMeta
        };

        string route = $"/productFamilies/{existingFamily.StringId}/relationships/tickets";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();

        store.Document.Meta.Should().BeEquivalentToJson(documentMeta);

        store.Document.Data.ManyValue.Should().HaveCount(2);

        store.Document.Data.ManyValue[0].Meta.Should().BeEquivalentToJson(identifierMeta1);
        store.Document.Data.ManyValue[1].Meta.Should().BeEquivalentToJson(identifierMeta2);
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
