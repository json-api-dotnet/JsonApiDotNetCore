using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public class EndToEndTest
    {
        public static MediaTypeHeaderValue JsonApiContentType = new MediaTypeHeaderValue("application/vnd.api+json");
        private HttpClient _client;
        protected TestFixture<Startup> _fixture;
        protected readonly IResponseDeserializer _deserializer;
        public EndToEndTest(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _deserializer = GetDeserializer();
        }

        public AppDbContext PrepareTest<TStartup>() where TStartup : class
        {
            var builder = new WebHostBuilder().UseStartup<TStartup>();
            var server = new TestServer(builder);
            _client = server.CreateClient();

            var dbContext = GetDbContext();
            dbContext.RemoveRange(dbContext.TodoItems);
            dbContext.RemoveRange(dbContext.TodoItemCollections);
            dbContext.RemoveRange(dbContext.PersonRoles);
            dbContext.RemoveRange(dbContext.People);
            dbContext.SaveChanges();
            return dbContext;
        }

        public AppDbContext GetDbContext()
        {
            return _fixture.GetService<AppDbContext>();
        }

        public async Task<(string, HttpResponseMessage)> SendRequest(string method, string route, string content)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), route);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = JsonApiContentType;
            var response = await _client.SendAsync(request);
            var body = await response.Content?.ReadAsStringAsync();
            return (body, response);
        }

        public Task<(string, HttpResponseMessage)> Post(string route, string content)
        {
            return SendRequest("POST", route, content);
        }

        public Task<(string, HttpResponseMessage)> Patch(string route, string content)
        {
            return SendRequest("PATCH", route, content);
        }

        public IRequestSerializer GetSerializer<TResource>(Expression<Func<TResource, dynamic>> attributes = null, Expression<Func<TResource, dynamic>> relationships = null) where TResource : class, IIdentifiable
        {
            return _fixture.GetSerializer(attributes, relationships);
        }

        public IResponseDeserializer GetDeserializer()
        {
            return _fixture.GetDeserializer();
        }

        protected void AssertEqualStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            Assert.True(expected == response.StatusCode, $"Got {response.StatusCode} status code with payload instead of {expected}. Payload: {response.Content.ReadAsStringAsync().Result}");
        }
    }

}
