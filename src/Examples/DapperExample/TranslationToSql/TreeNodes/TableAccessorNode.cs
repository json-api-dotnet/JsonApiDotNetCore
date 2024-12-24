namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the base type for accessors to tabular data, such as FROM and JOIN.
/// </summary>
internal abstract class TableAccessorNode : SqlTreeNode
{
    public TableSourceNode Source { get; }

    protected TableAccessorNode(TableSourceNode source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;
    }
}
