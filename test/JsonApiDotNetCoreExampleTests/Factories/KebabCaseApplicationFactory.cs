using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;

namespace JsonApiDotNetCoreExampleTests
{
    public class KebabCaseApplicationFactory : CustomApplicationFactoryBase
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseStartup<KebabCaseStartup>();
        }
    }
}
