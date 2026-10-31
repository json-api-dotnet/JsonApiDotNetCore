using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

internal static class DocumentIdExtensions
{
    /// <summary>
    /// Returns the IDs of the primary resources, in document order.
    /// </summary>
    public static string?[] GetPrimaryIds(this Document document)
    {
        document.Data.ManyValue.Should().NotBeNull();
        return document.Data.ManyValue.Select(resource => resource.Id).ToArray();
    }

    /// <summary>
    /// Returns the IDs of the included resources, in document order.
    /// </summary>
    public static string?[] GetIncludedIds(this Document document)
    {
        document.Included.Should().NotBeNull();
        return document.Included.Select(resource => resource.Id).ToArray();
    }

    /// <summary>
    /// Returns the IDs in the to-many relationship data of a resource, in document order.
    /// </summary>
    public static string?[] GetLinkageIds(this ResourceObject resource, string relationshipName)
    {
        resource.Relationships.Should().ContainKey(relationshipName);
        RelationshipObject? relationship = resource.Relationships[relationshipName];

        relationship.Should().NotBeNull();
        relationship.Data.ManyValue.Should().NotBeNull();
        return relationship.Data.ManyValue.Select(identifier => identifier.Id).ToArray();
    }
}
