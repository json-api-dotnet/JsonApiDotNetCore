using System;
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
    public class NullValuedAttributeHandlingTests : IAsyncLifetime
    {
        private readonly TestFixture<Startup> _fixture;
        private readonly AppDbContext _dbContext;
        private readonly TodoItem _todoItem;

        public NullValuedAttributeHandlingTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _dbContext = fixture.GetService<AppDbContext>();
            _todoItem = new TodoItem
            {
                Description = null,
                Ordinal = 1,
                CreatedDate = DateTime.Now,
                AchievedDate = DateTime.Now.AddDays(2)
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
        [InlineData(null, true, "foo", false)]
        [InlineData(null, false, "foo", false)]
        [InlineData(true, true, "foo", true)]
        [InlineData(true, false, "foo", true)]
        [InlineData(null, true, null, false)]
        [InlineData(null, false, null, false)]
        public async Task CheckNullBehaviorCombination(bool? omitNullValuedAttributes, bool? allowClientOverride,
            string clientOverride, bool omitsNulls)
        {

            // Override some null handling options
            NullAttributeResponseBehavior nullAttributeResponseBehavior;
            if (omitNullValuedAttributes.HasValue && allowClientOverride.HasValue)
            {
                nullAttributeResponseBehavior = new NullAttributeResponseBehavior(omitNullValuedAttributes.Value, allowClientOverride.Value);
            }
            else if (omitNullValuedAttributes.HasValue)
            {
                nullAttributeResponseBehavior = new NullAttributeResponseBehavior(omitNullValuedAttributes.Value);
            }
            else if (allowClientOverride.HasValue)
            {
                nullAttributeResponseBehavior = new NullAttributeResponseBehavior(allowClientOverride: allowClientOverride.Value);
            }
            else
            {
                nullAttributeResponseBehavior = new NullAttributeResponseBehavior();
            }
            var jsonApiOptions = _fixture.GetService<JsonApiOptions>();
            jsonApiOptions.NullAttributeResponseBehavior = nullAttributeResponseBehavior;
            jsonApiOptions.AllowCustomQueryParameters = true;

            var httpMethod = new HttpMethod("GET");
            var queryString = allowClientOverride.HasValue
                ? $"?omitNullValuedAttributes={clientOverride}"
                : "";
            var route = $"/api/v1/todo-items/{_todoItem.Id}{queryString}"; 
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            // assert. does response contain a null valued attribute
            Assert.Equal(omitsNulls, !deserializeBody.Data.Attributes.ContainsKey("description"));

        }
    }

}
