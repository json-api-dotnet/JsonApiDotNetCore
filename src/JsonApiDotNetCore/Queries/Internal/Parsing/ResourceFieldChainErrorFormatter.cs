using System.Text;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

internal sealed class ResourceFieldChainErrorFormatter
{
    public string GetForNotFound(ResourceFieldCategory category, string publicName, ResourceType resourceType,
        FieldChainInheritanceRequirement inheritanceRequirement)
    {
        var builder = new StringBuilder();
        WriteSource(category, publicName, builder);

        builder.Append($" does not exist on resource type '{resourceType.PublicName}'");

        if (inheritanceRequirement != FieldChainInheritanceRequirement.Disabled && resourceType.DirectlyDerivedTypes.Any())
        {
            builder.Append(" or any of its derived types");
        }

        builder.Append('.');

        return builder.ToString();
    }

    public string GetForMultipleMatches(ResourceFieldCategory category, string publicName)
    {
        var builder = new StringBuilder();
        WriteSource(category, publicName, builder);

        builder.Append(" is defined on multiple derived types.");

        return builder.ToString();
    }

    public string GetForWrongFieldType(ResourceFieldCategory category, string publicName, ResourceType resourceType, string expected)
    {
        var builder = new StringBuilder();
        WriteSource(category, publicName, builder);

        builder.Append($" must be {expected} on resource type '{resourceType.PublicName}'.");

        return builder.ToString();
    }

    public string GetForNoneFound(ResourceFieldCategory category, string publicName, ICollection<ResourceType> parentResourceTypes, bool hasDerivedTypes)
    {
        var builder = new StringBuilder();
        WriteSource(category, publicName, builder);

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
}
