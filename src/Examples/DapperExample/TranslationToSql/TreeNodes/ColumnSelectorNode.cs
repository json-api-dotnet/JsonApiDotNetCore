namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a column selector in a <see cref="SelectNode" />. For example, <code><![CDATA[
/// t2.Id AS Id0
/// ]]></code> in:
/// <code><![CDATA[
/// SELECT t2.Id AS Id0 FROM (SELECT t1.Id FROM Users AS t1) AS t2
/// ]]></code>.
/// </summary>
internal sealed class ColumnSelectorNode : SelectorNode
{
    public ColumnNode Column { get; }

    public string Identity => Alias ?? Column.Name;

    public ColumnSelectorNode(ColumnNode column, string? alias)
        : base(alias)
    {
        ArgumentNullException.ThrowIfNull(column);

        Column = column;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitColumnSelector(this, argument);
    }
}
