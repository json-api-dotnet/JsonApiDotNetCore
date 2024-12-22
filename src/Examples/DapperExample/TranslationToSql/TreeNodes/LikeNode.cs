using JsonApiDotNetCore.Queries.Expressions;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a sub-string match filter. For example: <code><![CDATA[
/// t1.Name LIKE 'A%'
/// ]]></code>.
/// </summary>
internal sealed class LikeNode : FilterNode
{
    public ColumnNode Column { get; }
    public TextMatchKind MatchKind { get; }
    public string Text { get; }

    public LikeNode(ColumnNode column, TextMatchKind matchKind, string text)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentNullException.ThrowIfNull(text);

        Column = column;
        MatchKind = matchKind;
        Text = text;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitLike(this, argument);
    }
}
