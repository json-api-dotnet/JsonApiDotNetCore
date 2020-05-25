using System;
using System.Reflection;

namespace JsonApiDotNetCore.Models.Fluent
{
    public interface IResourceMappingService
    {
        void RegisterResourceMappings(Assembly assembly);

        void RegisterResourceMapping(IResourceMapping resourceMapping);

        bool TryGetResourceMapping(Type entityType, out IResourceMapping resourceMapping);        
    }
}
