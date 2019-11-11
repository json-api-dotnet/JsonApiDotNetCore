using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Graph;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreExampleTests
{
    public class KebabCaseApplicationFactory : CustomApplicationFactoryBase
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IResourceNameFormatter, KebabCaseFormatter>();
                services.AddClientSerialization();
            });
        }
    }
}
