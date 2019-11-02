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
        private IServiceProvider _services;
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
            var serializer =  GetService<IRequestSerializer>();
            var graph =  GetService<IResourceGraph>();
            serializer.AttributesToSerialize = graph.GetAttributes(attributes);
            serializer.RelationshipsToSerialize = graph.GetRelationships(attributes);
            return serializer;
        }
        public IResponseDeserializer GetDeserializer()
        {
            var resourceGraph = new ResourceGraphBuilder()
                .AddResource<PersonRole>()
                .AddResource<Article>()
                .AddResource<Tag>()
                .AddResource<CamelCasedModel>()
                .AddResource<User>()
                .AddResource<Person>()
                .AddResource<Author>()
                .AddResource<Passport>()
                .AddResource<TodoItemClient>("todo-items")
                .AddResource<TodoItemCollectionClient, Guid>().Build();
            return new ResponseDeserializer(resourceGraph);
        }

        public T GetService<T>() => (T)_services.GetService(typeof(T));
        
        public void ReloadDbContext()
        {
            Context = new AppDbContext(GetService<DbContextOptions<AppDbContext>>());
        }

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
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
