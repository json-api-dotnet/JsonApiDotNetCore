namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a reference to a column in a <see cref="TableNode" />. For example, <code><![CDATA[
/// t1.FirstName
/// ]]></code> in:
/// <code><![CDATA[
/// FROM Users AS t1
/// ]]></code>.
/// </summary>
internal sealed class ColumnInTableNode(string name, ColumnType type, string? tableAlias)
    : ColumnNode(name, type, tableAlias)
{
    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitColumnInTable(this, argument);
    }
}
