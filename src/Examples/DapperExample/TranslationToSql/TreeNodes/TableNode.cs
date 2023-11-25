using Humanizer;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a reference to a database table. For example, <code><![CDATA[
/// Users AS t2
/// ]]></code> in:
/// <code><![CDATA[
/// INNER JOIN Users AS t2 ON t1.UserId = t2.Id
/// ]]></code>.
/// </summary>
internal sealed class TableNode : TableSourceNode
{
    private readonly ResourceType _resourceType;
    private readonly IReadOnlyDictionary<string, ResourceFieldAttribute?> _columnMappings;
    private readonly List<ColumnInTableNode> _columns = new();

    public string Name => _resourceType.ClrType.Name.Pluralize();

    public override IReadOnlyList<ColumnInTableNode> Columns => _columns;

    public TableNode(ResourceType resourceType, IReadOnlyDictionary<string, ResourceFieldAttribute?> columnMappings, string? alias)
        : base(alias)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(columnMappings);

        _resourceType = resourceType;
        _columnMappings = columnMappings;

        ReadColumnMappings();
    }

    private void ReadColumnMappings()
    {
        foreach ((string columnName, ResourceFieldAttribute? field) in _columnMappings)
        {
            ColumnType columnType = field is RelationshipAttribute ? ColumnType.ForeignKey : ColumnType.Scalar;
            var column = new ColumnInTableNode(columnName, columnType, Alias);

            _columns.Add(column);
        }
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitTable(this, argument);
    }

    public override ColumnNode? FindColumn(string persistedColumnName, ColumnType? type, string? innerTableAlias)
    {
        if (innerTableAlias != Alias)
        {
            return null;
        }

        return Columns.FirstOrDefault(column => column.Name == persistedColumnName && (type == null || column.Type == type));
    }
}
