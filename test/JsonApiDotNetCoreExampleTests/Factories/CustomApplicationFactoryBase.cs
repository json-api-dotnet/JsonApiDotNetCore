using System;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace JsonApiDotNetCoreExampleTests
{
    public class CustomApplicationFactoryBase : WebApplicationFactory<Startup>, IApplicationFactory
    {
        public readonly HttpClient Client;
        private readonly IServiceScope _scope;

        public IServiceProvider ServiceProvider => _scope.ServiceProvider;

        public CustomApplicationFactoryBase()
        {
            Client = CreateClient();
            _scope = Services.CreateScope();
        }

        public T GetService<T>() => (T)_scope.ServiceProvider.GetService(typeof(T));
    }

    public interface IApplicationFactory
    {
        IServiceProvider ServiceProvider { get; }

        T GetService<T>();
        HttpClient CreateClient();
    }
}
