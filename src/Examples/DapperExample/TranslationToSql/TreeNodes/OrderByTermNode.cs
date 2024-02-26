namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the base type for terms in an <see cref="OrderByNode" />.
/// </summary>
internal abstract class OrderByTermNode(bool isAscending) : SqlTreeNode
{
    public bool IsAscending { get; } = isAscending;
}
