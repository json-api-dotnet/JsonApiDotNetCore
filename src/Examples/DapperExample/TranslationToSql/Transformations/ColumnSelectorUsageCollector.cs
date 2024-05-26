using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.Transformations;

/// <summary>
/// Collects all <see cref="ColumnNode" />s in selectors that are referenced elsewhere in the query.
/// </summary>
internal sealed partial class ColumnSelectorUsageCollector : SqlTreeNodeVisitor<ColumnVisitMode, object?>
{
    private readonly HashSet<ColumnNode> _usedColumns = [];
    private readonly ILogger<ColumnSelectorUsageCollector> _logger;

    public ISet<ColumnNode> UsedColumns => _usedColumns;

    public ColumnSelectorUsageCollector(ILoggerFactory loggerFactory)
    {
        ArgumentGuard.NotNull(loggerFactory);

        _logger = loggerFactory.CreateLogger<ColumnSelectorUsageCollector>();
    }

    public void Collect(SelectNode select)
    {
        ArgumentGuard.NotNull(select);

        LogStarted();

        _usedColumns.Clear();
        InnerVisit(select, ColumnVisitMode.Reference);

        LogFinished();
    }

    public override object? VisitSelect(SelectNode node, ColumnVisitMode mode)
    {
        foreach ((TableAccessorNode tableAccessor, IReadOnlyList<SelectorNode> tableSelectors) in node.Selectors)
        {
            InnerVisit(tableAccessor, mode);
            VisitSequence(tableSelectors, ColumnVisitMode.Declaration);
        }

        InnerVisit(node.Where, mode);
        InnerVisit(node.OrderBy, mode);
        return null;
    }

    public override object? VisitFrom(FromNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Source, mode);
        return null;
    }

    public override object? VisitJoin(JoinNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Source, mode);
        InnerVisit(node.OuterColumn, mode);
        InnerVisit(node.InnerColumn, mode);
        return null;
    }

    public override object? VisitColumnInSelect(ColumnInSelectNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Selector, ColumnVisitMode.Reference);
        return null;
    }

    public override object? VisitColumnSelector(ColumnSelectorNode node, ColumnVisitMode mode)
    {
        if (mode == ColumnVisitMode.Reference)
        {
            _usedColumns.Add(node.Column);
            LogColumnAdded(node.Column);
        }

        InnerVisit(node.Column, mode);
        return null;
    }

    public override object? VisitWhere(WhereNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Filter, mode);
        return null;
    }

    public override object? VisitNot(NotNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Child, mode);
        return null;
    }

    public override object? VisitLogical(LogicalNode node, ColumnVisitMode mode)
    {
        VisitSequence(node.Terms, mode);
        return null;
    }

    public override object? VisitComparison(ComparisonNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Left, mode);
        InnerVisit(node.Right, mode);
        return null;
    }

    public override object? VisitLike(LikeNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Column, mode);
        return null;
    }

    public override object? VisitIn(InNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Column, mode);
        VisitSequence(node.Values, mode);
        return null;
    }

    public override object? VisitExists(ExistsNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.SubSelect, mode);
        return null;
    }

    public override object? VisitCount(CountNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.SubSelect, mode);
        return null;
    }

    public override object? VisitOrderBy(OrderByNode node, ColumnVisitMode mode)
    {
        VisitSequence(node.Terms, mode);
        return null;
    }

    public override object? VisitOrderByColumn(OrderByColumnNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Column, mode);
        return null;
    }

    public override object? VisitOrderByCount(OrderByCountNode node, ColumnVisitMode mode)
    {
        InnerVisit(node.Count, mode);
        return null;
    }

    private void InnerVisit(SqlTreeNode? node, ColumnVisitMode mode)
    {
        if (node != null)
        {
            Visit(node, mode);
        }
    }

    private void VisitSequence(IEnumerable<SqlTreeNode> nodes, ColumnVisitMode mode)
    {
        foreach (SqlTreeNode node in nodes)
        {
            InnerVisit(node, mode);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Started collection of used columns.")]
    private partial void LogStarted();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Finished collection of used columns.")]
    private partial void LogFinished();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Added used column {Column}.")]
    private partial void LogColumnAdded(ColumnNode column);
}
