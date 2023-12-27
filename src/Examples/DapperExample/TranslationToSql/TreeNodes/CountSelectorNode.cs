namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a row count selector in a <see cref="SelectNode" />. For example, <code><![CDATA[
/// COUNT(*)
/// ]]></code> in:
/// <code><![CDATA[
/// SELECT COUNT(*) FROM Users
/// ]]></code>.
/// </summary>
internal sealed class CountSelectorNode(string? alias) : SelectorNode(alias)
{
    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitCountSelector(this, argument);
    }
}
