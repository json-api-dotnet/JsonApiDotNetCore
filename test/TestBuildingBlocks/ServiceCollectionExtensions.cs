using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace TestBuildingBlocks
{
    public static class ServiceCollectionExtensions
    {
        public static void AddControllersFromExampleProject(this IServiceCollection x)
        {

        }

        public static void UseControllers(this IServiceCollection services, TestControllerProvider testControllerProvider)
        {
            ArgumentGuard.NotNull(services, nameof(services));

            services.AddMvcCore().ConfigureApplicationPartManager(manager =>
            {
                RemoveExistingControllerFeatureProviders(manager);
                AddControllerAssemblies(testControllerProvider, manager);

                manager.FeatureProviders.Add(testControllerProvider);
            });
        }

        public static void RemoveControllerFeatureProviders(this IServiceCollection services)
        {
            ArgumentGuard.NotNull(services, nameof(services));

            services.AddMvcCore().ConfigureApplicationPartManager(manager =>
            {
                IApplicationFeatureProvider<ControllerFeature>[] providers = manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>()
                    .ToArray();

                foreach (IApplicationFeatureProvider<ControllerFeature> provider in providers)
                {
                    manager.FeatureProviders.Remove(provider);
                }
            });
        }

        private static void AddControllerAssemblies(TestControllerProvider testControllerProvider, ApplicationPartManager manager)
        {
            HashSet<AssemblyPart> controllerAssemblies = testControllerProvider.NamespaceEntryPoints.Select(type => new AssemblyPart(type.Assembly)).ToHashSet();

            controllerAssemblies.UnionWith(testControllerProvider.AllowedControllerTypes.Select(type => new AssemblyPart(type.Assembly)));

            foreach (AssemblyPart part in controllerAssemblies)
            {
                manager.ApplicationParts.Add(part);
            }
        }

        private static void RemoveExistingControllerFeatureProviders(ApplicationPartManager manager)
        {
            IApplicationFeatureProvider<ControllerFeature>[] providers = manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().ToArray();

            foreach (IApplicationFeatureProvider<ControllerFeature> provider in providers)
            {
                manager.FeatureProviders.Remove(provider);
            }
        }
    }
}
