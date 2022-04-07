using System.Text;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

internal sealed class ResourceFieldChainErrorFormatter
{
    public string GetForNotFound(ResourceFieldCategory category, string publicName, string path, ResourceType resourceType,
        FieldChainInheritanceRequirement inheritanceRequirement)
    {
        var builder = new StringBuilder();
        WriteSource(category, publicName, builder);
        WritePath(path, publicName, builder);

        builder.Append($" does not exist on resource type '{resourceType.PublicName}'");

        if (inheritanceRequirement != FieldChainInheritanceRequirement.Disabled && resourceType.DirectlyDerivedTypes.Any())
        {
            builder.Append(" or any of its derived types");
        }

        builder.Append('.');

        return builder.ToString();
    }

    public string GetForMultipleMatches(ResourceFieldCategory category, string publicName, string path)
    {
        var builder = new StringBuilder();
        WriteSource(category, publicName, builder);
        WritePath(path, publicName, builder);

        builder.Append(" is defined on multiple derived types.");

        return builder.ToString();
    }

    public string GetForWrongFieldType(ResourceFieldCategory category, string publicName, string path, ResourceType resourceType, string expected)
    {
        var builder = new StringBuilder();
        WriteSource(category, publicName, builder);
        WritePath(path, publicName, builder);

        builder.Append($" must be {expected} on resource type '{resourceType.PublicName}'.");

        return builder.ToString();
    }

    public string GetForNoneFound(ResourceFieldCategory category, string publicName, string path, ICollection<ResourceType> parentResourceTypes,
        bool hasDerivedTypes)
    {
        var builder = new StringBuilder();
        WriteSource(category, publicName, builder);
        WritePath(path, publicName, builder);

        if (parentResourceTypes.Count == 1)
        {
            builder.Append($" does not exist on resource type '{parentResourceTypes.First().PublicName}'");
        }
        else
        {
            string typeNames = string.Join(", ", parentResourceTypes.Select(type => $"'{type.PublicName}'"));
            builder.Append($" does not exist on any of the resource types {typeNames}");
        }

        builder.Append(hasDerivedTypes ? " or any of its derived types." : ".");

        return builder.ToString();
    }

    private static void WriteSource(ResourceFieldCategory category, string publicName, StringBuilder builder)
    {
        builder.Append($"{category} '{publicName}'");
    }

    private static void WritePath(string path, string publicName, StringBuilder builder)
    {
        if (path != publicName)
        {
            builder.Append($" in '{path}'");
        }
    }
}
