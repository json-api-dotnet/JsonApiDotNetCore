namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the base type for terms in an <see cref="OrderByNode" />.
/// </summary>
internal abstract class OrderByTermNode : SqlTreeNode
{
    public bool IsAscending { get; }

    protected OrderByTermNode(bool isAscending)
    {
        IsAscending = isAscending;
    }
}
