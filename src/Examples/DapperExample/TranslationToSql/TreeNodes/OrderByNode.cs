using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents an ORDER BY clause. For example: <code><![CDATA[
/// ORDER BY t1.LastName, t1.LastModifiedAt DESC
/// ]]></code>.
/// </summary>
internal sealed class OrderByNode : SqlTreeNode
{
    public IReadOnlyList<OrderByTermNode> Terms { get; }

    public OrderByNode(IReadOnlyList<OrderByTermNode> terms)
    {
        ArgumentGuard.NotNullNorEmpty(terms);

        Terms = terms;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitOrderBy(this, argument);
    }
}
