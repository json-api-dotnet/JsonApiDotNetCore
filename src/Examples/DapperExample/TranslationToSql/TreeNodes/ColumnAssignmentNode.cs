using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents assignment to a column in an <see cref="UpdateNode" />. For example, <code><![CDATA[
/// FirstName = @p1
/// ]]></code> in:
/// <code><![CDATA[
/// UPDATE Customers SET FirstName = @p1 WHERE Id = @p2
/// ]]></code>.
/// </summary>
internal sealed class ColumnAssignmentNode : SqlTreeNode
{
    public ColumnNode Column { get; }
    public SqlValueNode Value { get; }

    public ColumnAssignmentNode(ColumnNode column, SqlValueNode value)
    {
        ArgumentGuard.NotNull(column);
        ArgumentGuard.NotNull(value);

        Column = column;
        Value = value;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitColumnAssignment(this, argument);
    }
}
