using System.Reflection;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NoDbConsoleQueryExample.Repositories;

/// <summary>
/// Provides access to resource types and their fields, when not using Entity Framework Core.
/// </summary>
internal sealed class ResourceModel : RuntimeModel
{
    public ResourceModel(IResourceGraph resourceGraph)
    {
        foreach (ResourceType resourceType in resourceGraph.GetResourceTypes())
        {
            RuntimeEntityType entityType = AddEntityType(resourceType.ClrType.FullName!, resourceType.ClrType);
            SetEntityProperties(entityType, resourceType);
        }
    }

    private static void SetEntityProperties(RuntimeEntityType entityType, ResourceType resourceType)
    {
        foreach (PropertyInfo property in resourceType.ClrType.GetProperties())
        {
            entityType.AddProperty(property.Name, property.PropertyType, property);
        }
    }
}
