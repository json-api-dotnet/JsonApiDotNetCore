using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks
{
    public sealed class ResourceHooksStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        public ResourceHooksStartup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISystemClock, FrozenSystemClock>();

            base.ConfigureServices(services);

            services.AddControllersFromTestProject();
            services.AddClientSerialization();
        }

        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.Namespace = "api/v1";
            options.EnableResourceHooks = true;
            options.LoadDatabaseValues = true;
        }
    }
}
