using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace TestBuildingBlocks;

internal static class ServiceCollectionExtensions
{
    public static void ReplaceControllers(this IServiceCollection services, TestControllerProvider provider)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(provider);

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
