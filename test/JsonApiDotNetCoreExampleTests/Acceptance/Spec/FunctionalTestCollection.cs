using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Client.Internal;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
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
            _dbContext = _factory.GetRequiredService<AppDbContext>();
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
                builder.Add(rc.ResourceType, rc.IdentityType, rc.PublicName);
            }
            builder.Add<TodoItemClient>(formatter.FormatResourceName(typeof(TodoItem)));
            builder.Add<TodoItemCollectionClient, Guid>(formatter.FormatResourceName(typeof(TodoItemCollection)));
            return new ResponseDeserializer(builder.Build(), new ResourceFactory(_factory.ServiceProvider));
        }

        protected AppDbContext GetDbContext() => GetService<AppDbContext>();

        protected T GetService<T>() => _factory.GetRequiredService<T>();

        protected void AssertEqualStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            var responseBody = response.Content.ReadAsStringAsync().Result;
            Assert.True(expected == response.StatusCode, $"Got {response.StatusCode} status code instead of {expected}. Response body: {responseBody}");
        }

        protected void ClearDbContext()
        {
            _dbContext.ClearTable<TodoItem>();
            _dbContext.ClearTable<TodoItemCollection>();
            _dbContext.ClearTable<PersonRole>();
            _dbContext.ClearTable<Person>();
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
