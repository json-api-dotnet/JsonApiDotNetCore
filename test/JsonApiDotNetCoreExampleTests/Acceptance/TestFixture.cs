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
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal.Contracts;

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


        public IRequestSerializer GetSerializer<TResource>(Expression<Func<TResource, dynamic>> attributes = null, Expression<Func<TResource, dynamic>> relationships = null) where TResource : class, IIdentifiable
        {
            var serializer = GetService<IRequestSerializer>();
            var graph = GetService<IResourceGraph>();
            if (attributes != null)
                serializer.AttributesToSerialize = graph.GetAttributes(attributes);
            if (relationships != null)
                serializer.RelationshipsToSerialize = graph.GetRelationships(relationships);
            return serializer;
        }
        public IResponseDeserializer GetDeserializer()
        {
            var resourceGraph = new ResourceGraphBuilder()
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
