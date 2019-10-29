using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
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
            Context = Server.GetService<AppDbContext>();
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
            var resourceGraph = new ResourceGraphBuilder()
                .AddResource<PersonRole>()
                .AddResource<Article>()
                .AddResource<Tag>()
                .AddResource<CamelCasedModel>()
                .AddResource<User>()
                .AddResource<Person>()
                .AddResource<Author>()
                .AddResource<Passport>()
                .AddResource<TodoItemClient>("custom-todo-items")
                .AddResource<TodoItemCollectionClient, Guid>().Build();
            return new ResponseDeserializer(resourceGraph);
        }

        public T GetService<T>() => (T)_services.GetService(typeof(T));

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
