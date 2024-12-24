using JsonApiDotNetCore.Queries.Expressions;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the comparison of two values. For example: <code><![CDATA[
/// t1.Age >= @p1
/// ]]></code>.
/// </summary>
internal sealed class ComparisonNode : FilterNode
{
    public ComparisonOperator Operator { get; }
    public SqlValueNode Left { get; }
    public SqlValueNode Right { get; }

    public ComparisonNode(ComparisonOperator @operator, SqlValueNode left, SqlValueNode right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        Operator = @operator;
        Left = left;
        Right = right;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitComparison(this, argument);
    }
}
