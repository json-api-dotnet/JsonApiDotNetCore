using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace JsonApiDotNetCoreExampleTests
{
    public class StandardApplicationFactory : CustomApplicationFactoryBase
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddClientSerialization();
            });
        }
    }
}
