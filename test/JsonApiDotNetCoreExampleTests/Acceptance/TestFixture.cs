using System;
using System.Net;
using System.Net.Http;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public class TestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly TestServer _server;
        public readonly IServiceProvider ServiceProvider;
        public TestFixture()
        {
            var builder = WebHost.CreateDefaultBuilder().UseStartup<TStartup>();
            _server = new TestServer(builder);
            ServiceProvider = _server.Host.Services;

            Client = _server.CreateClient();
            Context = GetRequiredService<IDbContextResolver>().GetContext() as AppDbContext;
        }

        public HttpClient Client { get; set; }
        public AppDbContext Context { get; }

        public T GetRequiredService<T>() => (T)ServiceProvider.GetRequiredService(typeof(T));

        public void AssertEqualStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            var responseBody = response.Content.ReadAsStringAsync().Result;
            Assert.True(expected == response.StatusCode, $"Got {response.StatusCode} status code instead of {expected}. Response body: {responseBody}");
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
