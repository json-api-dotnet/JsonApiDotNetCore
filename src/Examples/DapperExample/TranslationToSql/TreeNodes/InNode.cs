using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a filter that matches one value in a candidate set. For example: <code><![CDATA[
/// t1.Priority IN ('High', 'Medium')
/// ]]></code>.
/// </summary>
internal sealed class InNode : FilterNode
{
    public ColumnNode Column { get; }
    public IReadOnlyList<SqlValueNode> Values { get; }

    public InNode(ColumnNode column, IReadOnlyList<SqlValueNode> values)
    {
        ArgumentGuard.NotNull(column);
        ArgumentGuard.NotNullNorEmpty(values);

        Column = column;
        Values = values;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitIn(this, argument);
    }
}
