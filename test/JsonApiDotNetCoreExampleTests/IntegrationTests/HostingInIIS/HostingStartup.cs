using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.HostingInIIS
{
    public sealed class HostingStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        public HostingStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.Namespace = "public-api";
            options.IncludeTotalResourceCount = true;
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UsePathBase("/iis-application-virtual-directory");

            base.Configure(app, environment);
        }
    }
}
