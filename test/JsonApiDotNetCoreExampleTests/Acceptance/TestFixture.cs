using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public class TestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly TestServer _server;
        public readonly IServiceProvider ServiceProvider;
        public TestFixture()
        {
            var builder = new WebHostBuilder().UseStartup<TStartup>();
            _server = new TestServer(builder);
            ServiceProvider = _server.Host.Services;
            Client = _server.CreateClient();
            Context = GetService<IDbContextResolver>().GetContext() as AppDbContext;
        }

        public HttpClient Client { get; set; }
        public AppDbContext Context { get; private set; }

        public static IRequestSerializer GetSerializer<TResource>(IServiceProvider serviceProvider, Expression<Func<TResource, dynamic>> attributes = null, Expression<Func<TResource, dynamic>> relationships = null) where TResource : class, IIdentifiable
        {
            var serializer = (IRequestSerializer)serviceProvider.GetService(typeof(IRequestSerializer));
            var graph = (IResourceGraph)serviceProvider.GetService(typeof(IResourceGraph));
            serializer.AttributesToSerialize = attributes != null ? graph.GetAttributes(attributes) : null;
            serializer.RelationshipsToSerialize = relationships != null ? graph.GetRelationships(relationships) : null;
            return serializer;
        }

        public IRequestSerializer GetSerializer<TResource>(Expression<Func<TResource, dynamic>> attributes = null, Expression<Func<TResource, dynamic>> relationships = null) where TResource : class, IIdentifiable
        {
            var serializer = GetService<IRequestSerializer>();
            var graph = GetService<IResourceGraph>();
            serializer.AttributesToSerialize = attributes != null ? graph.GetAttributes(attributes) : null;
            serializer.RelationshipsToSerialize = relationships != null ? graph.GetRelationships(relationships) : null;
            return serializer;
        }

        public IResponseDeserializer GetDeserializer()
        {
            var options = GetService<IJsonApiOptions>();
            var resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance)
                .AddResource<PersonRole>()
                .AddResource<Article>()
                .AddResource<Tag>()
                .AddResource<KebabCasedModel>()
                .AddResource<User>()
                .AddResource<SuperUser>()
                .AddResource<Person>()
                .AddResource<Author>()
                .AddResource<Passport>()
                .AddResource<TodoItemClient>("todoItems")
                .AddResource<TodoItemCollectionClient, Guid>().Build();
            return new ResponseDeserializer(resourceGraph, new DefaultResourceFactory(ServiceProvider));
        }

        public T GetService<T>() => (T)ServiceProvider.GetService(typeof(T));

        public void ReloadDbContext()
        {
            ISystemClock systemClock = ServiceProvider.GetRequiredService<ISystemClock>();
            DbContextOptions<AppDbContext> options = GetService<DbContextOptions<AppDbContext>>();
            
            Context = new AppDbContext(options, systemClock);
        }

        public void AssertEqualStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            Assert.True(expected == response.StatusCode, $"Got {response.StatusCode} status code with payload instead of {expected}. Payload: {response.Content.ReadAsStringAsync().Result}");
        }

        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Client.Dispose();
                    _server.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
