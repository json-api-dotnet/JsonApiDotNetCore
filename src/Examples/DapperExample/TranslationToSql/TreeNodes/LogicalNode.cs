using JsonApiDotNetCore;
using JsonApiDotNetCore.Queries.Expressions;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a logical AND/OR filter. For example: <code><![CDATA[
/// (t1.StartTime IS NOT NULL) AND (t1.FinishTime IS NULL)
/// ]]></code>.
/// </summary>
internal sealed class LogicalNode : FilterNode
{
    public LogicalOperator Operator { get; }
    public IReadOnlyList<FilterNode> Terms { get; }

    public LogicalNode(LogicalOperator @operator, params FilterNode[] terms)
        : this(@operator, terms.AsReadOnly())
    {
    }

    public LogicalNode(LogicalOperator @operator, IReadOnlyList<FilterNode> terms)
    {
        ArgumentGuard.NotNull(terms);

        if (terms.Count < 2)
        {
            throw new ArgumentException("At least two terms are required.", nameof(terms));
        }

        Operator = @operator;
        Terms = terms;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitLogical(this, argument);
    }
}
