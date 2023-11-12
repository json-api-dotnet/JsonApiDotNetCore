using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the base type for accessors to tabular data, such as FROM and JOIN.
/// </summary>
internal abstract class TableAccessorNode : SqlTreeNode
{
    public TableSourceNode Source { get; }

    protected TableAccessorNode(TableSourceNode source)
    {
        ArgumentGuard.NotNull(source);

        Source = source;
    }
}
