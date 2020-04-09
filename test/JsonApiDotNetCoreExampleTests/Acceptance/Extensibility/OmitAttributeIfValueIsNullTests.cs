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
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    [Collection("WebHostCollection")]
    public sealed class OmitAttributeIfValueIsNullTests : IAsyncLifetime
    {
        private readonly TestFixture<Startup> _fixture;
        private readonly AppDbContext _dbContext;
        private readonly TodoItem _todoItem;

        public OmitAttributeIfValueIsNullTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _dbContext = fixture.GetService<AppDbContext>();
            var person = new Person { FirstName = "Bob", LastName = null };
            _todoItem = new TodoItem
            {
                Description = null,
                Ordinal = 1,
                CreatedDate = DateTime.Now,
                AchievedDate = DateTime.Now.AddDays(2),
                Owner = person
            };
            _todoItem = _dbContext.TodoItems.Add(_todoItem).Entity;
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
        [InlineData(null, null, null, false)]
        [InlineData(true, null, null, true)]
        [InlineData(false, true, "true", true)]
        [InlineData(false, false, "true", false)]
        [InlineData(true, true, "false", false)]
        [InlineData(true, false, "false", true)]
        [InlineData(null, false, "false", false)]
        [InlineData(null, false, "true", false)]
        [InlineData(null, true, "true", true)]
        [InlineData(null, true, "false", false)]
        [InlineData(null, true, "this-is-not-a-boolean-value", false)]
        [InlineData(null, false, "this-is-not-a-boolean-value", false)]
        [InlineData(true, true, "this-is-not-a-boolean-value", true)]
        [InlineData(true, false, "this-is-not-a-boolean-value", true)]
        [InlineData(null, true, null, false)]
        [InlineData(null, false, null, false)]
        public async Task CheckNullBehaviorCombination(bool? omitAttributeIfValueIsNull, bool? allowQueryStringOverride,
            string queryStringOverride, bool expectNullsMissing)
        {
            var jsonApiOptions = _fixture.GetService<IJsonApiOptions>();
            if (omitAttributeIfValueIsNull != null)
            {
                jsonApiOptions.SerializerSettings.NullValueHandling = omitAttributeIfValueIsNull.Value
                    ? NullValueHandling.Ignore
                    : NullValueHandling.Include;
            }
            if (allowQueryStringOverride != null)
            {
                jsonApiOptions.AllowOmitNullQueryStringOverride = allowQueryStringOverride.Value;
            }

            var httpMethod = new HttpMethod("GET");
            var queryString = allowQueryStringOverride != null
                ? $"&omitNull={queryStringOverride}"
                : "";
            var route = $"/api/v1/todoItems/{_todoItem.Id}?include=owner{queryString}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            var isQueryStringMissing = queryString.Length > 0 && queryStringOverride == null;
            var isQueryStringInvalid = queryString.Length > 0 && queryStringOverride != null && !bool.TryParse(queryStringOverride, out _);
            var isDisallowedOverride = allowQueryStringOverride == false && queryStringOverride != null;

            if (isDisallowedOverride)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
                Assert.Single(errorDocument.Errors);
                Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
                Assert.Equal("Usage of one or more query string parameters is not allowed at the requested endpoint.", errorDocument.Errors[0].Title);
                Assert.Equal("The parameter 'omitNull' cannot be used at this endpoint.", errorDocument.Errors[0].Detail);
                Assert.Equal("omitNull", errorDocument.Errors[0].Source.Parameter);
            }
            else if (isQueryStringMissing)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
                Assert.Single(errorDocument.Errors);
                Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
                Assert.Equal("Missing query string parameter value.", errorDocument.Errors[0].Title);
                Assert.Equal("Missing value for 'omitNull' query string parameter.", errorDocument.Errors[0].Detail);
                Assert.Equal("omitNull", errorDocument.Errors[0].Source.Parameter);
            }
            else if (isQueryStringInvalid)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
                Assert.Single(errorDocument.Errors);
                Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
                Assert.Equal("The specified query string value must be 'true' or 'false'.", errorDocument.Errors[0].Title);
                Assert.Equal("The value 'this-is-not-a-boolean-value' for parameter 'omitNull' is not a valid boolean value.", errorDocument.Errors[0].Detail);
                Assert.Equal("omitNull", errorDocument.Errors[0].Source.Parameter);
            }
            else
            {
                // Assert: does response contain a null valued attribute?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var deserializeBody = JsonConvert.DeserializeObject<Document>(body);
                Assert.Equal(expectNullsMissing, !deserializeBody.SingleData.Attributes.ContainsKey("description"));
                Assert.Equal(expectNullsMissing, !deserializeBody.Included[0].Attributes.ContainsKey("lastName"));
            }
        }
    }
}
