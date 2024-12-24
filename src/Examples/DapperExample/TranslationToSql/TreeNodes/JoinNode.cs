namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a JOIN clause. For example: <code><![CDATA[
/// LEFT JOIN Customers AS t2 ON t1.CustomerId = t2.Id
/// ]]></code>.
/// </summary>
internal sealed class JoinNode : TableAccessorNode
{
    public JoinType JoinType { get; }
    public ColumnNode OuterColumn { get; }
    public ColumnNode InnerColumn { get; }

    public JoinNode(JoinType joinType, TableSourceNode source, ColumnNode outerColumn, ColumnNode innerColumn)
        : base(source)
    {
        ArgumentNullException.ThrowIfNull(outerColumn);
        ArgumentNullException.ThrowIfNull(innerColumn);

        JoinType = joinType;
        OuterColumn = outerColumn;
        InnerColumn = innerColumn;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitJoin(this, argument);
    }
}
