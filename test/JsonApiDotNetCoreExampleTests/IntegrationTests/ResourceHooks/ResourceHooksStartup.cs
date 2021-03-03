using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class ResourceHooksStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddControllersFromExampleProject();
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
