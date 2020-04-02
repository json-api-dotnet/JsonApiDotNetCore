using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
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

            // Override some null handling options
            NullAttributeResponseBehavior nullAttributeResponseBehavior;
            if (omitAttributeIfValueIsNull.HasValue && allowQueryStringOverride.HasValue)
                nullAttributeResponseBehavior = new NullAttributeResponseBehavior(omitAttributeIfValueIsNull.Value, allowQueryStringOverride.Value);
            else if (omitAttributeIfValueIsNull.HasValue)
                nullAttributeResponseBehavior = new NullAttributeResponseBehavior(omitAttributeIfValueIsNull.Value);
            else if (allowQueryStringOverride.HasValue)
                nullAttributeResponseBehavior = new NullAttributeResponseBehavior(allowQueryStringOverride: allowQueryStringOverride.Value);
            else
                nullAttributeResponseBehavior = new NullAttributeResponseBehavior();

            var jsonApiOptions = _fixture.GetService<IJsonApiOptions>();
            jsonApiOptions.NullAttributeResponseBehavior = nullAttributeResponseBehavior;

            var httpMethod = new HttpMethod("GET");
            var queryString = allowQueryStringOverride.HasValue
                ? $"&omitNull={queryStringOverride}"
                : "";
            var route = $"/api/v1/todoItems/{_todoItem.Id}?include=owner{queryString}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            if (queryString.Length > 0 && !bool.TryParse(queryStringOverride, out _))
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
            else if (allowQueryStringOverride == false && queryStringOverride != null)
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
            else
            {
                // Assert: does response contain a null valued attribute?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                Assert.Equal(expectNullsMissing, !deserializeBody.SingleData.Attributes.ContainsKey("description"));
                Assert.Equal(expectNullsMissing, !deserializeBody.Included[0].Attributes.ContainsKey("lastName"));
            }
        }
    }
}
