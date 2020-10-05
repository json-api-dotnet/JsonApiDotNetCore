using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreExampleTests.Helpers.Extensions
{
    public static class TestServerExtensions
    {
        public static T GetRequiredService<T>(this TestServer server)
        {
            return (T)server.Host.Services.GetRequiredService(typeof(T));
        }
    }
}
