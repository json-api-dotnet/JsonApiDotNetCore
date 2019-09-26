using System;
using System.Net.Http;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Contracts;

using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Data;
using Microsoft.EntityFrameworkCore;

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
            deserializer = GetService<IJsonApiDeserializer>();
            JsonApiContext = GetService<IJsonApiContext>();
        }

        public HttpClient Client { get; set; }
        public AppDbContext Context { get; private set; }
        public IJsonApiDeserializer deserializer { get; private set; }
        public IJsonApiContext JsonApiContext { get; private set; }
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