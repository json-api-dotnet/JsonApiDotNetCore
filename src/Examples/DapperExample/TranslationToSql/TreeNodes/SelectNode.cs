using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents a SELECT clause, which is a shaped selection of rows from database tables. For example:
/// <code><![CDATA[
/// SELECT t1.FirstName, t1.LastName
/// FROM Users AS t1
/// WHERE t1.Age > @p1
/// ORDER BY t1.Age, t1.LastName
/// ]]></code>.
/// </summary>
internal sealed class SelectNode : TableSourceNode
{
    private readonly List<ColumnInSelectNode> _columns = [];

    public IReadOnlyDictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> Selectors { get; }
    public WhereNode? Where { get; }
    public OrderByNode? OrderBy { get; }

    public override IReadOnlyList<ColumnInSelectNode> Columns => _columns.AsReadOnly();

    public SelectNode(IReadOnlyDictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> selectors, WhereNode? where, OrderByNode? orderBy, string? alias)
        : base(alias)
    {
        ArgumentGuard.NotNullNorEmpty(selectors);

        Selectors = selectors;
        Where = where;
        OrderBy = orderBy;

        ReadSelectorColumns(selectors);
    }

    private void ReadSelectorColumns(IReadOnlyDictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> selectors)
    {
        foreach (ColumnSelectorNode columnSelector in selectors.SelectMany(selector => selector.Value).OfType<ColumnSelectorNode>())
        {
            var column = new ColumnInSelectNode(columnSelector, Alias);
            _columns.Add(column);
        }
    }

    public override ColumnNode? FindColumn(string persistedColumnName, ColumnType? type, string? innerTableAlias)
    {
        if (innerTableAlias == Alias)
        {
            return Columns.FirstOrDefault(column => column.GetPersistedColumnName() == persistedColumnName && (type == null || column.Type == type));
        }

        foreach (TableSourceNode tableSource in Selectors.Keys.Select(tableAccessor => tableAccessor.Source))
        {
            ColumnNode? innerColumn = tableSource.FindColumn(persistedColumnName, type, innerTableAlias);

            if (innerColumn != null)
            {
                ColumnInSelectNode outerColumn = Columns.Single(column => column.Selector.Column == innerColumn);
                return outerColumn;
            }
        }

        return null;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitSelect(this, argument);
    }
}
