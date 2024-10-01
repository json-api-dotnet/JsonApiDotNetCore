using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a reference to a column in a <see cref="SelectNode" />. For example, <code><![CDATA[
/// t2.Id0
/// ]]></code> in:
/// <code><![CDATA[
/// SELECT t2.Id AS Id0 FROM (SELECT t1.Id FROM Users AS t1) AS t2
/// ]]></code>.
/// </summary>
internal sealed class ColumnInSelectNode(ColumnSelectorNode selector, string? tableAlias)
    : ColumnNode(GetColumnName(selector), selector.Column.Type, tableAlias)
{
    public ColumnSelectorNode Selector { get; } = selector;

    public bool IsVirtual => Selector.Alias != null || Selector.Column is ColumnInSelectNode { IsVirtual: true };

    private static string GetColumnName(ColumnSelectorNode selector)
    {
        ArgumentGuard.NotNull(selector);

        return selector.Identity;
    }

    public string GetPersistedColumnName()
    {
        return Selector.Column is ColumnInSelectNode columnInSelect ? columnInSelect.GetPersistedColumnName() : Selector.Column.Name;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitColumnInSelect(this, argument);
    }
}
