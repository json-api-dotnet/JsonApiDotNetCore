using Microsoft.AspNetCore.TestHost;

namespace JsonApiDotNetCoreExampleTests.Helpers.Extensions
{
    public static class TestServerExtensions
    {
        public static T GetService<T>(this TestServer server)
        {
            return (T)server.Host.Services.GetService(typeof(T));
        }
    }
}