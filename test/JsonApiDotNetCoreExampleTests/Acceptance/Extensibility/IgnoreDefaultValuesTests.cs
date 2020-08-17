using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    [Collection("WebHostCollection")]
    public sealed class IgnoreDefaultValuesTests : IAsyncLifetime
    {
        private readonly AppDbContext _dbContext;
        private readonly TodoItem _todoItem;

        public IgnoreDefaultValuesTests(TestFixture<TestStartup> fixture)
        {
            _dbContext = fixture.GetService<AppDbContext>();
            _todoItem = new TodoItem
            {
                CreatedDate = default,
                Owner = new Person { Age = default }
            };
            _dbContext.TodoItems.Add(_todoItem);
        }

        public async Task InitializeAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(null, null, null, DefaultValueHandling.Include)]
        [InlineData(null, null, "false", DefaultValueHandling.Include)]
        [InlineData(null, null, "true", DefaultValueHandling.Include)]
        [InlineData(null, null, "unknown", null)]
        [InlineData(null, null, "", null)]
        [InlineData(null, false, null, DefaultValueHandling.Include)]
        [InlineData(null, false, "false", DefaultValueHandling.Include)]
        [InlineData(null, false, "true", DefaultValueHandling.Include)]
        [InlineData(null, false, "unknown", null)]
        [InlineData(null, false, "", null)]
        [InlineData(null, true, null, DefaultValueHandling.Include)]
        [InlineData(null, true, "false", DefaultValueHandling.Ignore)]
        [InlineData(null, true, "true", DefaultValueHandling.Include)]
        [InlineData(null, true, "unknown", null)]
        [InlineData(null, true, "", null)]
        [InlineData(DefaultValueHandling.Ignore, null, null, DefaultValueHandling.Ignore)]
        [InlineData(DefaultValueHandling.Ignore, null, "false", DefaultValueHandling.Ignore)]
        [InlineData(DefaultValueHandling.Ignore, null, "true", DefaultValueHandling.Ignore)]
        [InlineData(DefaultValueHandling.Ignore, null, "unknown", null)]
        [InlineData(DefaultValueHandling.Ignore, null, "", null)]
        [InlineData(DefaultValueHandling.Ignore, false, null, DefaultValueHandling.Ignore)]
        [InlineData(DefaultValueHandling.Ignore, false, "false", DefaultValueHandling.Ignore)]
        [InlineData(DefaultValueHandling.Ignore, false, "true", DefaultValueHandling.Ignore)]
        [InlineData(DefaultValueHandling.Ignore, false, "unknown", null)]
        [InlineData(DefaultValueHandling.Ignore, false, "", null)]
        [InlineData(DefaultValueHandling.Ignore, true, null, DefaultValueHandling.Ignore)]
        [InlineData(DefaultValueHandling.Ignore, true, "false", DefaultValueHandling.Ignore)]
        [InlineData(DefaultValueHandling.Ignore, true, "true", DefaultValueHandling.Include)]
        [InlineData(DefaultValueHandling.Ignore, true, "unknown", null)]
        [InlineData(DefaultValueHandling.Ignore, true, "", null)]
        [InlineData(DefaultValueHandling.Include, null, null, DefaultValueHandling.Include)]
        [InlineData(DefaultValueHandling.Include, null, "false", DefaultValueHandling.Include)]
        [InlineData(DefaultValueHandling.Include, null, "true", DefaultValueHandling.Include)]
        [InlineData(DefaultValueHandling.Include, null, "unknown", null)]
        [InlineData(DefaultValueHandling.Include, null, "", null)]
        [InlineData(DefaultValueHandling.Include, false, null, DefaultValueHandling.Include)]
        [InlineData(DefaultValueHandling.Include, false, "false", DefaultValueHandling.Include)]
        [InlineData(DefaultValueHandling.Include, false, "true", DefaultValueHandling.Include)]
        [InlineData(DefaultValueHandling.Include, false, "unknown", null)]
        [InlineData(DefaultValueHandling.Include, false, "", null)]
        [InlineData(DefaultValueHandling.Include, true, null, DefaultValueHandling.Include)]
        [InlineData(DefaultValueHandling.Include, true, "false", DefaultValueHandling.Ignore)]
        [InlineData(DefaultValueHandling.Include, true, "true", DefaultValueHandling.Include)]
        [InlineData(DefaultValueHandling.Include, true, "unknown", null)]
        [InlineData(DefaultValueHandling.Include, true, "", null)]
        public async Task CheckBehaviorCombination(DefaultValueHandling? defaultValue, bool? allowQueryStringOverride, string queryStringValue, DefaultValueHandling? expected)
        {
            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var services = server.Host.Services;
            var client = server.CreateClient();

            var options = (JsonApiOptions)services.GetService(typeof(IJsonApiOptions));

            if (defaultValue != null)
            {
                options.SerializerSettings.DefaultValueHandling = defaultValue.Value;
            }
            if (allowQueryStringOverride != null)
            {
                options.AllowQueryStringOverrideForSerializerDefaultValueHandling = allowQueryStringOverride.Value;
            }

            var queryString = queryStringValue != null
                ? $"&defaults={queryStringValue}"
                : "";
            var route = $"/api/v1/todoItems/{_todoItem.Id}?include=owner{queryString}";
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            var isQueryStringValueEmpty = queryStringValue == string.Empty;
            var isDisallowedOverride = options.AllowQueryStringOverrideForSerializerDefaultValueHandling == false && queryStringValue != null;
            var isQueryStringInvalid = queryStringValue != null && !bool.TryParse(queryStringValue, out _);

            if (isQueryStringValueEmpty)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
                Assert.Single(errorDocument.Errors);
                Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
                Assert.Equal("Missing query string parameter value.", errorDocument.Errors[0].Title);
                Assert.Equal("Missing value for 'defaults' query string parameter.", errorDocument.Errors[0].Detail);
                Assert.Equal("defaults", errorDocument.Errors[0].Source.Parameter);
            }
            else if (isDisallowedOverride)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
                Assert.Single(errorDocument.Errors);
                Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
                Assert.Equal("Usage of one or more query string parameters is not allowed at the requested endpoint.", errorDocument.Errors[0].Title);
                Assert.Equal("The parameter 'defaults' cannot be used at this endpoint.", errorDocument.Errors[0].Detail);
                Assert.Equal("defaults", errorDocument.Errors[0].Source.Parameter);
            }
            else if (isQueryStringInvalid)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
                Assert.Single(errorDocument.Errors);
                Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
                Assert.Equal("The specified defaults is invalid.", errorDocument.Errors[0].Title);
                Assert.Equal("The value 'unknown' must be 'true' or 'false'.", errorDocument.Errors[0].Detail);
                Assert.Equal("defaults", errorDocument.Errors[0].Source.Parameter);
            }
            else
            {
                if (expected == null)
                {
                    throw new Exception("Invalid test combination. Should never get here.");
                }

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var deserializeBody = JsonConvert.DeserializeObject<Document>(body);
                Assert.Equal(expected == DefaultValueHandling.Include, deserializeBody.SingleData.Attributes.ContainsKey("createdDate"));
                Assert.Equal(expected == DefaultValueHandling.Include, deserializeBody.Included[0].Attributes.ContainsKey("the-Age"));
            }
        }
    }
}
