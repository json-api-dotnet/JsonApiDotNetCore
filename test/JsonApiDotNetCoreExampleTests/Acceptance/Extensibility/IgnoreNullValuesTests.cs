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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    [Collection("WebHostCollection")]
    public sealed class IgnoreNullValuesTests : IAsyncLifetime
    {
        private readonly AppDbContext _dbContext;
        private readonly TodoItem _todoItem;

        public IgnoreNullValuesTests(TestFixture<Startup> fixture)
        {
            _dbContext = fixture.GetService<AppDbContext>();
            var todoItem = new TodoItem
            {
                Description = null,
                Ordinal = 1,
                CreatedDate = DateTime.Now,
                AchievedDate = DateTime.Now.AddDays(2),
                Owner = new Person { FirstName = "Bob", LastName = null }
            };
            _todoItem = _dbContext.TodoItems.Add(todoItem).Entity;
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
        [InlineData(null, null, null, NullValueHandling.Include)]
        [InlineData(null, null, "false", NullValueHandling.Include)]
        [InlineData(null, null, "true", NullValueHandling.Include)]
        [InlineData(null, null, "unknown", null)]
        [InlineData(null, null, "", null)]
        [InlineData(null, false, null, NullValueHandling.Include)]
        [InlineData(null, false, "false", NullValueHandling.Include)]
        [InlineData(null, false, "true", NullValueHandling.Include)]
        [InlineData(null, false, "unknown", null)]
        [InlineData(null, false, "", null)]
        [InlineData(null, true, null, NullValueHandling.Include)]
        [InlineData(null, true, "false", NullValueHandling.Ignore)]
        [InlineData(null, true, "true", NullValueHandling.Include)]
        [InlineData(null, true, "unknown", null)]
        [InlineData(null, true, "", null)]
        [InlineData(NullValueHandling.Ignore, null, null, NullValueHandling.Ignore)]
        [InlineData(NullValueHandling.Ignore, null, "false", NullValueHandling.Ignore)]
        [InlineData(NullValueHandling.Ignore, null, "true", NullValueHandling.Ignore)]
        [InlineData(NullValueHandling.Ignore, null, "unknown", null)]
        [InlineData(NullValueHandling.Ignore, null, "", null)]
        [InlineData(NullValueHandling.Ignore, false, null, NullValueHandling.Ignore)]
        [InlineData(NullValueHandling.Ignore, false, "false", NullValueHandling.Ignore)]
        [InlineData(NullValueHandling.Ignore, false, "true", NullValueHandling.Ignore)]
        [InlineData(NullValueHandling.Ignore, false, "unknown", null)]
        [InlineData(NullValueHandling.Ignore, false, "", null)]
        [InlineData(NullValueHandling.Ignore, true, null, NullValueHandling.Ignore)]
        [InlineData(NullValueHandling.Ignore, true, "false", NullValueHandling.Ignore)]
        [InlineData(NullValueHandling.Ignore, true, "true", NullValueHandling.Include)]
        [InlineData(NullValueHandling.Ignore, true, "unknown", null)]
        [InlineData(NullValueHandling.Ignore, true, "", null)]
        [InlineData(NullValueHandling.Include, null, null, NullValueHandling.Include)]
        [InlineData(NullValueHandling.Include, null, "false", NullValueHandling.Include)]
        [InlineData(NullValueHandling.Include, null, "true", NullValueHandling.Include)]
        [InlineData(NullValueHandling.Include, null, "unknown", null)]
        [InlineData(NullValueHandling.Include, null, "", null)]
        [InlineData(NullValueHandling.Include, false, null, NullValueHandling.Include)]
        [InlineData(NullValueHandling.Include, false, "false", NullValueHandling.Include)]
        [InlineData(NullValueHandling.Include, false, "true", NullValueHandling.Include)]
        [InlineData(NullValueHandling.Include, false, "unknown", null)]
        [InlineData(NullValueHandling.Include, false, "", null)]
        [InlineData(NullValueHandling.Include, true, null, NullValueHandling.Include)]
        [InlineData(NullValueHandling.Include, true, "false", NullValueHandling.Ignore)]
        [InlineData(NullValueHandling.Include, true, "true", NullValueHandling.Include)]
        [InlineData(NullValueHandling.Include, true, "unknown", null)]
        [InlineData(NullValueHandling.Include, true, "", null)]
        public async Task CheckBehaviorCombination(NullValueHandling? defaultValue, bool? allowQueryStringOverride, string queryStringValue, NullValueHandling? expected)
        {
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var services = server.Host.Services;
            var client = server.CreateClient();

            var options = (IJsonApiOptions)services.GetService(typeof(IJsonApiOptions));

            if (defaultValue != null)
            {
                options.SerializerSettings.NullValueHandling = defaultValue.Value;
            }
            if (allowQueryStringOverride != null)
            {
                options.AllowQueryStringOverrideForSerializerNullValueHandling = allowQueryStringOverride.Value;
            }

            var queryString = queryStringValue != null
                ? $"&nulls={queryStringValue}"
                : "";
            var route = $"/api/v1/todoItems/{_todoItem.Id}?include=owner{queryString}";
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            var isQueryStringValueEmpty = queryStringValue == string.Empty;
            var isDisallowedOverride = options.AllowQueryStringOverrideForSerializerNullValueHandling == false && queryStringValue != null;
            var isQueryStringInvalid = queryStringValue != null && !bool.TryParse(queryStringValue, out _);

            if (isQueryStringValueEmpty)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
                Assert.Single(errorDocument.Errors);
                Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
                Assert.Equal("Missing query string parameter value.", errorDocument.Errors[0].Title);
                Assert.Equal("Missing value for 'nulls' query string parameter.", errorDocument.Errors[0].Detail);
                Assert.Equal("nulls", errorDocument.Errors[0].Source.Parameter);
            }
            else if (isDisallowedOverride)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
                Assert.Single(errorDocument.Errors);
                Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
                Assert.Equal("Usage of one or more query string parameters is not allowed at the requested endpoint.", errorDocument.Errors[0].Title);
                Assert.Equal("The parameter 'nulls' cannot be used at this endpoint.", errorDocument.Errors[0].Detail);
                Assert.Equal("nulls", errorDocument.Errors[0].Source.Parameter);
            }
            else if (isQueryStringInvalid)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
                Assert.Single(errorDocument.Errors);
                Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
                Assert.Equal("The specified query string value must be 'true' or 'false'.", errorDocument.Errors[0].Title);
                Assert.Equal("The value 'unknown' for parameter 'nulls' is not a valid boolean value.", errorDocument.Errors[0].Detail);
                Assert.Equal("nulls", errorDocument.Errors[0].Source.Parameter);
            }
            else
            {
                if (expected == null)
                {
                    throw new Exception("Invalid test combination. Should never get here.");
                }

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var deserializeBody = JsonConvert.DeserializeObject<Document>(body);
                Assert.Equal(expected == NullValueHandling.Include, deserializeBody.SingleData.Attributes.ContainsKey("description"));
                Assert.Equal(expected == NullValueHandling.Include, deserializeBody.Included[0].Attributes.ContainsKey("lastName"));
            }
        }
    }
}
