using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a DELETE FROM clause. For example: <code><![CDATA[
/// DELETE FROM Customers WHERE Id = @p1
/// ]]></code>.
/// </summary>
internal sealed class DeleteNode : SqlTreeNode
{
    public TableNode Table { get; }
    public WhereNode Where { get; }

    public DeleteNode(TableNode table, WhereNode where)
    {
        ArgumentGuard.NotNull(table);
        ArgumentGuard.NotNull(where);

        Table = table;
        Where = where;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitDelete(this, argument);
    }
}
