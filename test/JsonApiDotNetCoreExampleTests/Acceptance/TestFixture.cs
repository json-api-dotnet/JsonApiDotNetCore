using System;
using System.Net.Http;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using JsonApiDotNetCore.Data;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Serialization.Client;
using System.Linq.Expressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal.Contracts;
using Microsoft.Extensions.Logging.Abstractions;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public class TestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly TestServer _server;
        private readonly IServiceProvider _services;
        public TestFixture()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TStartup>();
            _server = new TestServer(builder);
            _services = _server.Host.Services;

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
            return new ResponseDeserializer(resourceGraph);
        }

        public T GetService<T>() => (T)_services.GetService(typeof(T));

        public void ReloadDbContext()
        {
            Context = new AppDbContext(GetService<DbContextOptions<AppDbContext>>());
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
