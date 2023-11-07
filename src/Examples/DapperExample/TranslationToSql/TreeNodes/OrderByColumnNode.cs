using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents ordering on a column in an <see cref="OrderByNode" />. For example, <code><![CDATA[
/// t1.LastName DESC
/// ]]></code> in:
/// <code><![CDATA[
/// ORDER BY t1.LastName DESC
/// ]]></code>.
/// </summary>
internal sealed class OrderByColumnNode : OrderByTermNode
{
    public ColumnNode Column { get; }

    public OrderByColumnNode(ColumnNode column, bool isAscending)
        : base(isAscending)
    {
        ArgumentGuard.NotNull(column);

        Column = column;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitOrderByColumn(this, argument);
    }
}
