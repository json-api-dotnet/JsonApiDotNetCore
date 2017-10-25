using System;
using System.Net.Http;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using JsonApiDotNetCore.Services;
using Newtonsoft.Json;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public class TestFixture<TStartup> where TStartup : class
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
            Context = GetService<AppDbContext>();
            DeSerializer = GetService<IJsonApiDeSerializer>();
            JsonApiContext = GetService<IJsonApiContext>();
        }

        public HttpClient Client { get; set; }
        public AppDbContext Context { get; private set; }
        public IJsonApiDeSerializer DeSerializer { get; private set; }
        public IJsonApiContext JsonApiContext { get; private set; }
        public T GetService<T>() => (T)_services.GetService(typeof(T));
    }
}