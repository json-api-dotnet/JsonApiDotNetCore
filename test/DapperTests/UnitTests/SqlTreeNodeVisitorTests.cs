using System.Reflection;
using DapperExample.TranslationToSql;
using DapperExample.TranslationToSql.TreeNodes;
using FluentAssertions;
using Xunit;

namespace DapperTests.UnitTests;

public sealed class SqlTreeNodeVisitorTests
{
    [Fact]
    public void Visitor_methods_call_default_visit()
    {
        // Arrange
        var visitor = new TestVisitor();

        MethodInfo[] visitMethods = visitor.GetType().GetMethods()
            .Where(method => method.Name.StartsWith("Visit", StringComparison.Ordinal) && method.Name != "Visit").ToArray();

        object?[] parameters =
        [
            null,
            null
        ];

        // Act
        foreach (MethodInfo method in visitMethods)
        {
            _ = method.Invoke(visitor, parameters);
        }

        visitor.HitCount.Should().Be(26);
    }

    private sealed class TestVisitor : SqlTreeNodeVisitor<object?, object?>
    {
        public int HitCount { get; private set; }

        public override object? DefaultVisit(SqlTreeNode node, object? argument)
        {
            HitCount++;
            return base.DefaultVisit(node, argument);
        }
    }
}
