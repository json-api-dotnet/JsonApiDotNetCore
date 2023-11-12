using DapperExample.TranslationToSql.Builders;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the base type for all nodes in a SQL query.
/// </summary>
internal abstract class SqlTreeNode
{
    public abstract TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument);

    public override string ToString()
    {
        // This is only used for debugging purposes.
        var queryBuilder = new SqlQueryBuilder(DatabaseProvider.PostgreSql);
        return queryBuilder.GetCommand(this);
    }
}
