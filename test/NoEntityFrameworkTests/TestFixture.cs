using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Serialization.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using System;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
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

        public IResponseDeserializer GetDeserializer()
        {
            var options = GetService<IJsonApiOptions>();

            var resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).AddResource<TodoItem>("todoItems").Build();
            return new ResponseDeserializer(resourceGraph);
        }

        public T GetService<T>() => (T)_services.GetService(typeof(T));

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
