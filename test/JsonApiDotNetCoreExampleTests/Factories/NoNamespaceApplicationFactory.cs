using JsonApiDotNetCoreExample.Startups;
using Microsoft.AspNetCore.Hosting;

namespace JsonApiDotNetCoreExampleTests
{
    public class NoNamespaceApplicationFactory : CustomApplicationFactoryBase
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseStartup<NoNamespaceStartup>();
        }
    }
}
