using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents an UPDATE clause. For example: <code><![CDATA[
/// UPDATE Customers SET FirstName = @p1 WHERE Id = @p2
/// ]]></code>.
/// </summary>
internal sealed class UpdateNode : SqlTreeNode
{
    public TableNode Table { get; }
    public IReadOnlyCollection<ColumnAssignmentNode> Assignments { get; }
    public WhereNode Where { get; }

    public UpdateNode(TableNode table, IReadOnlyCollection<ColumnAssignmentNode> assignments, WhereNode where)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentGuard.NotNullNorEmpty(assignments);
        ArgumentNullException.ThrowIfNull(where);

        Table = table;
        Assignments = assignments;
        Where = where;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitUpdate(this, argument);
    }
}
