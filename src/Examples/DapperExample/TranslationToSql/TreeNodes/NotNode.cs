namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the logical negation of another filter. For example: <code><![CDATA[
/// NOT (StartDate IS NULL)
/// ]]></code>.
/// </summary>
internal sealed class NotNode : FilterNode
{
    public FilterNode Child { get; }

    public NotNode(FilterNode child)
    {
        ArgumentNullException.ThrowIfNull(child);

        Child = child;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitNot(this, argument);
    }
}
