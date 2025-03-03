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
                id = existingTicket.StringId
            },
            meta = new
            {
                category = "bug",
                priority = 1,
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
                }
            }
        };

        string route = $"/supportTickets/{existingTicket.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        store.Document.Should().NotBeNull();
        store.Document.Meta.Should().HaveCount(4);

        store.Document.Meta.Should().ContainKey("category").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be("bug");
        });

        store.Document.Meta.Should().ContainKey("priority").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetInt32().Should().Be(1);
        });

        store.Document.Meta.Should().ContainKey("components").WhoseValue.With(value =>
        {
            string innerJson = value.Should().BeOfType<JsonElement>().Subject.ToString();

            innerJson.Should().BeJson("""
                [
                  "login",
                  "single-sign-on"
                ]
                """);
        });

        store.Document.Meta.Should().ContainKey("relatedTo").WhoseValue.With(value =>
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
    }

    // TODO: Add more tests, creating a mixture of:
    // - Different endpoints: post resource, patch resource, post relationship, patch relationship, delete relationship, atomic:operations
    // - Meta at different depths in the request body
    //     For example, assert on store.Document.Data.SingleValue.Meta
    //     See IHasMeta usage at https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/openapi/src/JsonApiDotNetCore.OpenApi.Swashbuckle/JsonApiObjects for where meta can occur
    // - Varying data structures: primitive types such as string/int/bool, arrays, dictionaries, and nested combinations of them

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
