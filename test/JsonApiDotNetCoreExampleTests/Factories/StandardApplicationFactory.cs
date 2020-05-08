using JsonApiDotNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace JsonApiDotNetCoreExampleTests
{
    public class StandardApplicationFactory : CustomApplicationFactoryBase
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureTestServices(services =>
            {
                services.AddClientSerialization();
            });
        }
    }
}
