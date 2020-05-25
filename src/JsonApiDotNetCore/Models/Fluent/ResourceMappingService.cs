using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Models.Fluent
{
    public class ResourceMappingService: IResourceMappingService
    {
        private readonly IServiceCollection _services;
        
        public ResourceMappingService(IServiceCollection services)
        {
            _services = services;            
        }

        public void RegisterResourceMapping(IResourceMapping resourceMapping)
        {
            _services.AddSingleton(typeof(IResourceMapping), resourceMapping);
        }

        public void RegisterResourceMappings(Assembly assembly)
        {
            List<Type> resourceMappingTypes = assembly.GetTypes()
                                                      .Where(type => type.BaseType != null &&
                                                                     type.BaseType.IsGenericType &&
                                                                     type.BaseType.GetGenericTypeDefinition() == typeof(ResourceMapping<>) &&
                                                                     type.IsClass)
                                                      .Select(type => type)
                                                      .ToList();

            foreach (Type resourceMappingType in resourceMappingTypes)
            {
                IResourceMapping resourceMapping = (IResourceMapping)Activator.CreateInstance(resourceMappingType);

                RegisterResourceMapping(resourceMapping);
            }
        }

        public bool TryGetResourceMapping(Type entityType, out IResourceMapping resourceMapping)
        {                       
            var provider = _services.BuildServiceProvider();

            List<IResourceMapping> resourceMappings = provider.GetServices<IResourceMapping>()
                                                              .ToList();

            resourceMapping = resourceMappings.Where(x => x.GetType().BaseType != null &&
                                                          x.GetType().BaseType.IsGenericType &&
                                                          x.GetType().BaseType.GetGenericTypeDefinition() == typeof(ResourceMapping<>) &&
                                                          x.GetType().BaseType.GetGenericArguments()
                                                                              .Contains(entityType) &&
                                                          x.GetType().IsClass)
                                              .FirstOrDefault();

            return resourceMapping != null;
        }
    }
}
