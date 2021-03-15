using System.Linq;
using JsonApiDotNetCore;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests
{
    internal static class ServiceCollectionExtensions
    {
        public static void UseControllersFromNamespace(this IServiceCollection services, string @namespace, AssemblyPart assemblyWithNamespace = null)
        {
            ArgumentGuard.NotNull(@namespace, nameof(@namespace));

            services.AddMvcCore().ConfigureApplicationPartManager(manager =>
            {
                if (assemblyWithNamespace != null)
                {
                    manager.ApplicationParts.Add(assemblyWithNamespace);
                }

                ControllerFeatureProvider provider = manager.FeatureProviders.OfType<ControllerFeatureProvider>().First();
                manager.FeatureProviders.Remove(provider);

                manager.FeatureProviders.Add(new TestControllerProvider(@namespace));
            });
        }
    }
}
