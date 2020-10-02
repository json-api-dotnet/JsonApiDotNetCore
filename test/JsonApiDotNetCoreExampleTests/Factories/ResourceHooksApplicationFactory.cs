using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace JsonApiDotNetCoreExampleTests
{
    public class ResourceHooksApplicationFactory : CustomApplicationFactoryBase
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                services.AddClientSerialization();
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddJsonApi(options =>
                    {
                        options.Namespace = "api/v1";
                        options.DefaultPageSize = new PageSize(5);
                        options.IncludeTotalResourceCount = true;
                        options.EnableResourceHooks = true;
                        options.LoadDatabaseValues = true;
                        options.IncludeExceptionStackTraceInErrors = true;
                    },
                    discovery => discovery.AddAssembly(typeof(JsonApiDotNetCoreExample.Program).Assembly));
            });
        }
    }
}
