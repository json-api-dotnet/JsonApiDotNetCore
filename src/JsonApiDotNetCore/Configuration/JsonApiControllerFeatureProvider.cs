using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace JsonApiDotNetCore.Configuration
{
    public class JsonApiControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var currentAssembly = Assembly.GetCallingAssembly();

            var resourceDescriptors = currentAssembly
                .GetExportedTypes()
                .Select(TypeLocator.TryGetResourceDescriptor)
                .Where(descriptor => descriptor != null);

            foreach (var descriptor in resourceDescriptors)
            {
                feature.Controllers.Add(typeof(BaseJsonApiController<,>).MakeGenericType(descriptor.ResourceType, descriptor.IdType).GetTypeInfo());
            }
        }
    }
}
