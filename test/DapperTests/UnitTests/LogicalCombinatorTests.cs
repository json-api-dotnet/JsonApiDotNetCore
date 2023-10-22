using DapperExample.TranslationToSql.Transformations;
using DapperExample.TranslationToSql.TreeNodes;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Expressions;
using Xunit;

namespace DapperTests.UnitTests;

public sealed class LogicalCombinatorTests
{
    [Fact]
    public void Collapses_and_filters()
    {
        // Arrange
        var column = new ColumnInTableNode("column", ColumnType.Scalar, null);

        var conditionLeft1 = new ComparisonNode(ComparisonOperator.GreaterThan, column, new ParameterNode("@p1", 10));
        var conditionRight1 = new ComparisonNode(ComparisonOperator.LessThan, column, new ParameterNode("@p2", 20));
        var and1 = new LogicalNode(LogicalOperator.And, conditionLeft1, conditionRight1);

        var conditionLeft2 = new ComparisonNode(ComparisonOperator.GreaterOrEqual, column, new ParameterNode("@p3", 100));
        var conditionRight2 = new ComparisonNode(ComparisonOperator.LessOrEqual, column, new ParameterNode("@p4", 200));
        var and2 = new LogicalNode(LogicalOperator.And, conditionLeft2, conditionRight2);

        var conditionLeft3 = new LikeNode(column, TextMatchKind.EndsWith, "Z");
        var conditionRight3 = new LikeNode(column, TextMatchKind.StartsWith, "A");
        var and3 = new LogicalNode(LogicalOperator.And, conditionLeft3, conditionRight3);

        var source = new LogicalNode(LogicalOperator.And, and1, new LogicalNode(LogicalOperator.And, and2, and3));
        var combinator = new LogicalCombinator();

        // Act
        FilterNode result = combinator.Collapse(source);

        // Assert
        IEnumerable<string> terms = new FilterNode[]
        {
            conditionLeft1,
            conditionRight1,
            conditionLeft2,
            conditionRight2,
            conditionLeft3,
            conditionRight3
        }.Select(condition => condition.ToString());

        string expectedText = '(' + string.Join(") AND (", terms) + ')';
        result.ToString().Should().Be(expectedText);
    }
}
