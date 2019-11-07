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

        public CustomApplicationFactoryBase()
        {
            Client = CreateClient();
            _scope = Services.CreateScope();
        }

        public T GetService<T>() => (T)_scope.ServiceProvider.GetService(typeof(T));
    }

    public interface IApplicationFactory
    {
        T GetService<T>();
        HttpClient CreateClient();
    }
}
