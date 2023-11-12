using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the base type for references to columns in <see cref="TableSourceNode" />s.
/// </summary>
internal abstract class ColumnNode : SqlValueNode
{
    public string Name { get; }
    public ColumnType Type { get; }
    public string? TableAlias { get; }

    protected ColumnNode(string name, ColumnType type, string? tableAlias)
    {
        ArgumentGuard.NotNullNorEmpty(name);

        Name = name;
        Type = type;
        TableAlias = tableAlias;
    }

    public int GetTableAliasIndex()
    {
        if (TableAlias == null)
        {
            return -1;
        }

        string? number = TableAlias[1..];
        return int.Parse(number);
    }
}
