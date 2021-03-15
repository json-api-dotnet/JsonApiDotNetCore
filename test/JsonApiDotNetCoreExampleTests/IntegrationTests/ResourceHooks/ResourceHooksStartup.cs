using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExample.Controllers;
using JsonApiDotNetCoreExample.Startups;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
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
            var assemblyWithControllers = new AssemblyPart(typeof(EmptyStartup).Assembly);
            services.UseControllersFromNamespace(typeof(ArticlesController).Namespace, assemblyWithControllers);
            services.AddClientSerialization();
            services.AddJsonApi<TDbContext>(SetJsonApiOptions);
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
