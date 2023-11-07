using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a filter on whether a sub-query contains rows. For example, <code><![CDATA[
/// EXISTS (SELECT 1 FROM ...)
/// ]]></code> in:
/// <code><![CDATA[
/// WHERE t1.Name IS NOT NULL AND EXISTS (SELECT 1 FROM ...)
/// ]]></code>.
/// </summary>
internal sealed class ExistsNode : FilterNode
{
    public SelectNode SubSelect { get; }

    public ExistsNode(SelectNode subSelect)
    {
        ArgumentGuard.NotNull(subSelect);

        SubSelect = subSelect;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitExists(this, argument);
    }
}
