using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiRequestFormatMedataProvider
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class ApiExplorerStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            IMvcCoreBuilder builder = services.AddMvcCore().AddApiExplorer();
            builder.AddMvcOptions(options => options.Conventions.Add(new ApiExplorerConvention()));

            services.UseControllersFromNamespace(GetType().Namespace);

            services.AddJsonApi<TDbContext>(SetJsonApiOptions, mvcBuilder: builder);
        }
    }
}
