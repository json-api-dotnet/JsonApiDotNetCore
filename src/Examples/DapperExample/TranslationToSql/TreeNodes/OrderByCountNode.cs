using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents ordering on the number of rows returned from a sub-query in an <see cref="OrderByNode" />. For example,
/// <code><![CDATA[
/// (SELECT COUNT(*) FROM ...)
/// ]]></code> in: <code><![CDATA[
/// ORDER BY (SELECT COUNT(*) FROM ...)
/// ]]></code>.
/// </summary>
internal sealed class OrderByCountNode : OrderByTermNode
{
    public CountNode Count { get; }

    public OrderByCountNode(CountNode count, bool isAscending)
        : base(isAscending)
    {
        ArgumentGuard.NotNull(count);

        Count = count;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitOrderByCount(this, argument);
    }
}
