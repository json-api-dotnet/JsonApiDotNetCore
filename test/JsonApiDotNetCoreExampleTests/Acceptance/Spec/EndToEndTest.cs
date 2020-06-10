using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public class FunctionalTestCollection<TFactory> : IClassFixture<TFactory> where TFactory : class, IApplicationFactory
    {
        public static MediaTypeHeaderValue JsonApiContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);
        protected readonly TFactory _factory;
        protected readonly HttpClient _client;
        protected readonly AppDbContext _dbContext;
        protected IResponseDeserializer _deserializer;

        public FunctionalTestCollection(TFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _dbContext = _factory.GetService<AppDbContext>();
            _deserializer = GetDeserializer();
            ClearDbContext();
        }

        protected Task<(string, HttpResponseMessage)> Get(string route)
        {
            return SendRequest("GET", route);
        }

        protected Task<(string, HttpResponseMessage)> Post(string route, string content)
        {
            return SendRequest("POST", route, content);
        }

        protected Task<(string, HttpResponseMessage)> Patch(string route, string content)
        {
            return SendRequest("PATCH", route, content);
        }

        protected Task<(string, HttpResponseMessage)> Delete(string route)
        {
            return SendRequest("DELETE", route);
        }

        protected IRequestSerializer GetSerializer<TResource>(Expression<Func<TResource, dynamic>> attributes = null, Expression<Func<TResource, dynamic>> relationships = null) where TResource : class, IIdentifiable
        {
            var serializer = GetService<IRequestSerializer>();
            var graph = GetService<IResourceGraph>();
            serializer.AttributesToSerialize = attributes != null ? graph.GetAttributes(attributes) : null;
            serializer.RelationshipsToSerialize = relationships != null ? graph.GetRelationships(relationships) : null;
            return serializer;
        }

        protected IResponseDeserializer GetDeserializer()
        {
            var httpContextAccessor = GetService<IHttpContextAccessor>();
            var options = GetService<IJsonApiOptions>();
            var formatter = new ResourceNameFormatter(options);
            var resourcesContexts = GetService<IResourceGraph>().GetResourceContexts();
            var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
            foreach (var rc in resourcesContexts)
            {
                if (rc.ResourceType == typeof(TodoItem) || rc.ResourceType == typeof(TodoItemCollection))
                {
                    continue;
                }
                builder.AddResource(rc.ResourceType, rc.IdentityType, rc.ResourceName);
            }
            builder.AddResource<TodoItemClient>(formatter.FormatResourceName(typeof(TodoItem)));
            builder.AddResource<TodoItemCollectionClient, Guid>(formatter.FormatResourceName(typeof(TodoItemCollection)));
            return new ResponseDeserializer(builder.Build(), new DefaultResourceFactory(_factory.ServiceProvider));
        }

        protected AppDbContext GetDbContext() => GetService<AppDbContext>();

        protected T GetService<T>() => _factory.GetService<T>();

        protected void AssertEqualStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync();
            content.Wait();
            Assert.True(expected == response.StatusCode, $"Got {response.StatusCode} status code with payload instead of {expected}. Payload: {content.Result}");
        }

        protected void ClearDbContext()
        {
            _dbContext.RemoveRange(_dbContext.TodoItems);
            _dbContext.RemoveRange(_dbContext.TodoItemCollections);
            _dbContext.RemoveRange(_dbContext.PersonRoles);
            _dbContext.RemoveRange(_dbContext.People);
            _dbContext.SaveChanges();
        }

        private async Task<(string, HttpResponseMessage)> SendRequest(string method, string route, string content = null)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), route);
            if (content != null)
            {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = JsonApiContentType;
            }
            var response = await _client.SendAsync(request);
            var body = await response.Content?.ReadAsStringAsync();
            return (body, response);
        }
    }
}


namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public class EndToEndTest
    {
        public static MediaTypeHeaderValue JsonApiContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);
        private HttpClient _client;
        protected TestFixture<TestStartup> _fixture;
        protected readonly IResponseDeserializer _deserializer;
        public EndToEndTest(TestFixture<TestStartup> fixture)
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

        public async Task<(string, HttpResponseMessage)> SendRequest(string method, string route, string content = null)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), route);
            if (content != null)
            {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = JsonApiContentType;
            }
            var response = await _client.SendAsync(request);

            var body = await response.Content?.ReadAsStringAsync();
            return (body, response);
        }

        public Task<(string, HttpResponseMessage)> Get(string route)
        {
            return SendRequest("GET", route);
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
