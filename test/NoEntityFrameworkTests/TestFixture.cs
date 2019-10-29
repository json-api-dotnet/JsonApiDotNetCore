using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using System;
using System.Linq.Expressions;
using Startup = NoEntityFrameworkExample.Startup;

namespace NoEntityFrameworkTests
{
    public class TestFixture : IDisposable
    {
        public AppDbContext Context { get; private set; }
        public TestServer Server { get; private set; }
        private IServiceProvider _services;
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
            if (attributes != null)
                serializer.SetAttributesToSerialize(attributes);
            if (relationships != null)
                serializer.SetRelationshipsToSerialize(relationships);
            return serializer;
        }
        public IResponseDeserializer GetDeserializer()
        {
            var resourceGraph = new ResourceGraphBuilder().AddResource<TodoItem>("todo-items").Build();
            return new ResponseDeserializer(resourceGraph);
        }

        public T GetService<T>() => (T)_services.GetService(typeof(T));

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
