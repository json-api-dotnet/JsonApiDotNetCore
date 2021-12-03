using System.Reflection;
using JsonApiDotNetCore;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace TestBuildingBlocks
{
    internal static class ServiceCollectionExtensions
    {
        public static void ReplaceControllers(this IServiceCollection services, TestControllerProvider provider)
        {
            ArgumentGuard.NotNull(services, nameof(services));

            services.AddMvcCore().ConfigureApplicationPartManager(manager =>
            {
                RemoveExistingControllerFeatureProviders(manager);

                foreach (Assembly assembly in provider.ControllerAssemblies)
                {
                    manager.ApplicationParts.Add(new AssemblyPart(assembly));
                }

                manager.FeatureProviders.Add(provider);
            });
        }

        private static void RemoveExistingControllerFeatureProviders(ApplicationPartManager manager)
        {
            IApplicationFeatureProvider<ControllerFeature>[] providers = manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>()
                .ToArray();

            foreach (IApplicationFeatureProvider<ControllerFeature> provider in providers)
            {
                manager.FeatureProviders.Remove(provider);
            }
        }
    }
}
