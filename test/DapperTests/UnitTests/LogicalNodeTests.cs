using DapperExample.TranslationToSql.TreeNodes;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Expressions;
using Xunit;

namespace DapperTests.UnitTests;

public sealed class LogicalNodeTests
{
    [Fact]
    public void Throws_on_insufficient_terms()
    {
        // Arrange
        var filter = new ComparisonNode(ComparisonOperator.Equals, new ParameterNode("@p1", null), new ParameterNode("@p2", null));

        // Act
        Action action = () => _ = new LogicalNode(LogicalOperator.And, filter);

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithMessage("At least two terms are required.*");
    }
}
