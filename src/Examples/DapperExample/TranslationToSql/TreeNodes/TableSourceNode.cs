using JsonApiDotNetCore.Resources;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the base type for tabular data sources, such as database tables and sub-queries.
/// </summary>
internal abstract class TableSourceNode : SqlTreeNode
{
    public const string IdColumnName = nameof(Identifiable<object>.Id);

    public abstract IReadOnlyList<ColumnNode> Columns { get; }
    public string? Alias { get; }

    protected TableSourceNode(string? alias)
    {
        Alias = alias;
    }

    public ColumnNode GetIdColumn(string? innerTableAlias)
    {
        return GetColumn(IdColumnName, ColumnType.Scalar, innerTableAlias);
    }

    public ColumnNode GetColumn(string persistedColumnName, ColumnType? type, string? innerTableAlias)
    {
        ColumnNode? column = FindColumn(persistedColumnName, type, innerTableAlias);

        if (column == null)
        {
            throw new ArgumentException($"Column '{persistedColumnName}' not found.", nameof(persistedColumnName));
        }

        return column;
    }

    public abstract ColumnNode? FindColumn(string persistedColumnName, ColumnType? type, string? innerTableAlias);
}
