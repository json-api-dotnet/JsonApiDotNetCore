namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the base type for selectors in a <see cref="SelectNode" />.
/// </summary>
internal abstract class SelectorNode : SqlTreeNode
{
    public string? Alias { get; }

    protected SelectorNode(string? alias)
    {
        Alias = alias;
    }
}
