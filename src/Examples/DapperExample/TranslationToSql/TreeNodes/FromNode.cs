namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a FROM clause. For example: <code><![CDATA[
/// FROM Customers AS t1
/// ]]></code>.
/// </summary>
internal sealed class FromNode(TableSourceNode source) : TableAccessorNode(source)
{
    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitFrom(this, argument);
    }
}
