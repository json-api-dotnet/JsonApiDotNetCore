using System.Reflection;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NoEntityFrameworkExample.Data;

internal static class ResourceGraphExtensions
{
    public static IReadOnlyModel ToEntityModel(this IResourceGraph resourceGraph)
    {
        var modelBuilder = new ModelBuilder();

        foreach (ResourceType resourceType in resourceGraph.GetResourceTypes())
        {
            IncludeResourceType(resourceType, modelBuilder);
        }

        return modelBuilder.Model;
    }

    private static void IncludeResourceType(ResourceType resourceType, ModelBuilder builder)
    {
        EntityTypeBuilder entityTypeBuilder = builder.Entity(resourceType.ClrType);

        foreach (PropertyInfo property in resourceType.ClrType.GetProperties())
        {
            entityTypeBuilder.Property(property.PropertyType, property.Name);
        }
    }
}
