using DapperExample;
using DapperExample.TranslationToSql.DataModel;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DapperTests.UnitTests;

public sealed class RelationshipForeignKeyTests
{
    private readonly IResourceGraph _resourceGraph =
        new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<TestResource, long>().Build();

    [Fact]
    public void Can_format_foreign_key_for_ToOne_relationship()
    {
        // Arrange
        RelationshipAttribute parentRelationship = _resourceGraph.GetResourceType<TestResource>().GetRelationshipByPropertyName(nameof(TestResource.Parent));

        // Act
        var foreignKey = new RelationshipForeignKey(DatabaseProvider.PostgreSql, parentRelationship, true, "ParentId", true);

        // Assert
        foreignKey.ToString().Should().Be(@"TestResource.Parent => ""TestResources"".""ParentId""?");
    }

    [Fact]
    public void Can_format_foreign_key_for_ToMany_relationship()
    {
        // Arrange
        RelationshipAttribute childrenRelationship =
            _resourceGraph.GetResourceType<TestResource>().GetRelationshipByPropertyName(nameof(TestResource.Children));

        // Act
        var foreignKey = new RelationshipForeignKey(DatabaseProvider.PostgreSql, childrenRelationship, false, "TestResourceId", false);

        // Assert
        foreignKey.ToString().Should().Be(@"TestResource.Children => ""TestResources"".""TestResourceId""");
    }

    [UsedImplicitly]
    private sealed class TestResource : Identifiable<long>
    {
        [HasOne]
        public TestResource? Parent { get; set; }

        [HasMany]
        public ISet<TestResource> Children { get; set; } = new HashSet<TestResource>();
    }
}
