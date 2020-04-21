using JsonApiDotNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Reflection;

namespace JsonApiDotNetCoreExampleTests
{
    public class ClientEnabledIdsApplicationFactory : CustomApplicationFactoryBase
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddClientSerialization();
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddJsonApi(options =>
                {
                    options.Namespace = "api/v1";
                    options.DefaultPageSize = 5;
                    options.IncludeTotalRecordCount = true;
                    options.LoadDatabaseValues = true;
                    options.AllowClientGeneratedIds = true;
                },
                discovery => discovery.AddAssembly(Assembly.Load(nameof(JsonApiDotNetCoreExample))));
            });
        }
    }
}
