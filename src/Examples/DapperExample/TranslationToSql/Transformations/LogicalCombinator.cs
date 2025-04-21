using DapperExample.TranslationToSql.TreeNodes;

namespace DapperExample.TranslationToSql.Transformations;

/// <summary>
/// Collapses nested logical filters. This turns "A AND (B AND C)" into "A AND B AND C".
/// </summary>
internal sealed class LogicalCombinator : SqlTreeNodeVisitor<object?, SqlTreeNode>
{
    public FilterNode Collapse(FilterNode filter)
    {
        return TypedVisit(filter);
    }

    public override SqlTreeNode VisitLogical(LogicalNode node, object? argument)
    {
        List<FilterNode> newTerms = [];

        foreach (FilterNode newTerm in node.Terms.Select(TypedVisit))
        {
            if (newTerm is LogicalNode logicalTerm && logicalTerm.Operator == node.Operator)
            {
                newTerms.AddRange(logicalTerm.Terms);
            }
            else
            {
                newTerms.Add(newTerm);
            }
        }

        return new LogicalNode(node.Operator, newTerms.AsReadOnly());
    }

    public override SqlTreeNode DefaultVisit(SqlTreeNode node, object? argument)
    {
        return node;
    }

    public override SqlTreeNode VisitNot(NotNode node, object? argument)
    {
        FilterNode newChild = TypedVisit(node.Child);
        return new NotNode(newChild);
    }

    public override SqlTreeNode VisitComparison(ComparisonNode node, object? argument)
    {
        SqlValueNode newLeft = TypedVisit(node.Left);
        SqlValueNode newRight = TypedVisit(node.Right);

        return new ComparisonNode(node.Operator, newLeft, newRight);
    }

    private T TypedVisit<T>(T node)
        where T : SqlTreeNode
    {
        return (T)Visit(node, null);
    }
}
