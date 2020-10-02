using System;
using System.Net.Http;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreExampleTests
{
    public class CustomApplicationFactoryBase : WebApplicationFactory<TestStartup>, IApplicationFactory
    {
        public readonly HttpClient Client;
        private readonly IServiceScope _scope;

        public IServiceProvider ServiceProvider => _scope.ServiceProvider;

        public CustomApplicationFactoryBase()
        {
            Client = CreateClient();
            _scope = Services.CreateScope();
        }

        public T GetRequiredService<T>() => (T)_scope.ServiceProvider.GetRequiredService(typeof(T));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseStartup<TestStartup>();
        }
    }

    public interface IApplicationFactory
    {
        IServiceProvider ServiceProvider { get; }

        T GetRequiredService<T>();
        HttpClient CreateClient();
    }
}
