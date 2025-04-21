namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the ordinal selector for the first, unnamed column in a <see cref="SelectNode" />. For example, <code><![CDATA[
/// 1
/// ]]></code> in:
/// <code><![CDATA[
/// SELECT 1 FROM Users
/// ]]></code>.
/// </summary>
internal sealed class OneSelectorNode(string? alias)
    : SelectorNode(alias)
{
    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitOneSelector(this, argument);
    }
}
