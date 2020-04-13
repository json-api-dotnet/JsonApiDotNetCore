using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using System;
using System.Linq.Expressions;
using JsonApiDotNetCore.Configuration;
using NoEntityFrameworkExample;

namespace NoEntityFrameworkTests
{
    public class TestFixture : IDisposable
    {
        public AppDbContext Context { get; }
        public TestServer Server { get; }
        private readonly IServiceProvider _services;
        public TestFixture()
        {
            var builder = new WebHostBuilder().UseStartup<Startup>();
            Server = new TestServer(builder);
            Context = (AppDbContext)Server.Services.GetService(typeof(AppDbContext));
            Context.Database.EnsureCreated();
            _services = Server.Host.Services;
        }

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
            var options = GetService<IJsonApiOptions>();

            var resourceGraph = new ResourceGraphBuilder(options).AddResource<TodoItem>("todoItems").Build();
            return new ResponseDeserializer(resourceGraph);
        }

        public T GetService<T>() => (T)_services.GetService(typeof(T));

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
