using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace JsonApiDotNetCoreExampleTests
{
    public class TestFixture<TStartup> : IDisposable
        where TStartup : class
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
        }

        public HttpClient Client { get; set; }

        public T GetService<T>()
        {
            return (T)_services.GetService(typeof(T));
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