using System.Reflection;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NoEntityFrameworkExample.Data;

internal sealed class InMemoryModel : RuntimeModel
{
    public InMemoryModel(IResourceGraph resourceGraph)
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
            entityType.AddProperty(property.Name, property.PropertyType, propertyInfo: property);
        }
    }
}
