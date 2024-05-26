using System.Diagnostics.CodeAnalysis;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;

namespace DapperExample.TranslationToSql.Transformations;

/// <summary>
/// Removes unreferenced selectors in sub-queries.
/// </summary>
/// <example>
/// <p>
/// Regular query: <code><![CDATA[
/// SELECT t1."Id", t1."LastName"
/// FROM People AS t1
/// ]]></code>
/// </p>
/// <p>
/// Equivalent with sub-query:
/// <code><![CDATA[
/// SELECT t2."Id", t2."LastName"
/// FROM (
///     SELECT t1."Id", t1."AccountId", t1."FirstName", t1."LastName"
///     FROM People AS t1
/// ) AS t2
/// ]]></code>
/// </p>
/// The selectors t1."AccountId" and t1."FirstName" have no references and can be removed.
/// </example>
internal sealed partial class UnusedSelectorsRewriter : SqlTreeNodeVisitor<ISet<ColumnNode>, SqlTreeNode>
{
    private readonly ColumnSelectorUsageCollector _usageCollector;
    private readonly ILogger<UnusedSelectorsRewriter> _logger;
    private SelectNode _rootSelect = null!;
    private bool _hasChanged;

    public UnusedSelectorsRewriter(ILoggerFactory loggerFactory)
    {
        ArgumentGuard.NotNull(loggerFactory);

        _usageCollector = new ColumnSelectorUsageCollector(loggerFactory);
        _logger = loggerFactory.CreateLogger<UnusedSelectorsRewriter>();
    }

    public SelectNode RemoveUnusedSelectorsInSubQueries(SelectNode select)
    {
        ArgumentGuard.NotNull(select);

        _rootSelect = select;

        do
        {
            _hasChanged = false;
            _usageCollector.Collect(_rootSelect);

            LogStarted();
            _rootSelect = TypedVisit(_rootSelect, _usageCollector.UsedColumns);
            LogFinished();
        }
        while (_hasChanged);

        return _rootSelect;
    }

    public override SqlTreeNode DefaultVisit(SqlTreeNode node, ISet<ColumnNode> usedColumns)
    {
        return node;
    }

    public override SqlTreeNode VisitSelect(SelectNode node, ISet<ColumnNode> usedColumns)
    {
        IReadOnlyDictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> selectors = VisitSelectors(node, usedColumns);
        WhereNode? where = TypedVisit(node.Where, usedColumns);
        OrderByNode? orderBy = TypedVisit(node.OrderBy, usedColumns);
        return new SelectNode(selectors, where, orderBy, node.Alias);
    }

    private IReadOnlyDictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> VisitSelectors(SelectNode select, ISet<ColumnNode> usedColumns)
    {
        Dictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> newSelectors = [];

        foreach ((TableAccessorNode tableAccessor, IReadOnlyList<SelectorNode> tableSelectors) in select.Selectors)
        {
            TableAccessorNode newTableAccessor = TypedVisit(tableAccessor, usedColumns);
            IReadOnlyList<SelectorNode> newTableSelectors = select == _rootSelect ? tableSelectors : VisitTableSelectors(tableSelectors, usedColumns);
            newSelectors.Add(newTableAccessor, newTableSelectors);
        }

        return newSelectors;
    }

    private List<SelectorNode> VisitTableSelectors(IEnumerable<SelectorNode> selectors, ISet<ColumnNode> usedColumns)
    {
        List<SelectorNode> newTableSelectors = [];

        foreach (SelectorNode selector in selectors)
        {
            if (selector is ColumnSelectorNode columnSelector)
            {
                if (!usedColumns.Contains(columnSelector.Column))
                {
                    LogSelectorRemoved(columnSelector);
                    _hasChanged = true;
                    continue;
                }
            }

            newTableSelectors.Add(selector);
        }

        return newTableSelectors;
    }

    public override SqlTreeNode VisitFrom(FromNode node, ISet<ColumnNode> usedColumns)
    {
        TableSourceNode source = TypedVisit(node.Source, usedColumns);
        return new FromNode(source);
    }

    public override SqlTreeNode VisitJoin(JoinNode node, ISet<ColumnNode> usedColumns)
    {
        TableSourceNode source = TypedVisit(node.Source, usedColumns);
        ColumnNode outerColumn = TypedVisit(node.OuterColumn, usedColumns);
        ColumnNode innerColumn = TypedVisit(node.InnerColumn, usedColumns);
        return new JoinNode(node.JoinType, source, outerColumn, innerColumn);
    }

    public override SqlTreeNode VisitColumnInSelect(ColumnInSelectNode node, ISet<ColumnNode> usedColumns)
    {
        ColumnSelectorNode selector = TypedVisit(node.Selector, usedColumns);
        return new ColumnInSelectNode(selector, node.TableAlias);
    }

    public override SqlTreeNode VisitColumnSelector(ColumnSelectorNode node, ISet<ColumnNode> usedColumns)
    {
        ColumnNode column = TypedVisit(node.Column, usedColumns);
        return new ColumnSelectorNode(column, node.Alias);
    }

    public override SqlTreeNode VisitWhere(WhereNode node, ISet<ColumnNode> usedColumns)
    {
        FilterNode filter = TypedVisit(node.Filter, usedColumns);
        return new WhereNode(filter);
    }

    public override SqlTreeNode VisitNot(NotNode node, ISet<ColumnNode> usedColumns)
    {
        FilterNode child = TypedVisit(node.Child, usedColumns);
        return new NotNode(child);
    }

    public override SqlTreeNode VisitLogical(LogicalNode node, ISet<ColumnNode> usedColumns)
    {
        IReadOnlyList<FilterNode> terms = VisitList(node.Terms, usedColumns);
        return new LogicalNode(node.Operator, terms);
    }

    public override SqlTreeNode VisitComparison(ComparisonNode node, ISet<ColumnNode> usedColumns)
    {
        SqlValueNode left = TypedVisit(node.Left, usedColumns);
        SqlValueNode right = TypedVisit(node.Right, usedColumns);
        return new ComparisonNode(node.Operator, left, right);
    }

    public override SqlTreeNode VisitLike(LikeNode node, ISet<ColumnNode> usedColumns)
    {
        ColumnNode column = TypedVisit(node.Column, usedColumns);
        return new LikeNode(column, node.MatchKind, node.Text);
    }

    public override SqlTreeNode VisitIn(InNode node, ISet<ColumnNode> usedColumns)
    {
        ColumnNode column = TypedVisit(node.Column, usedColumns);
        IReadOnlyList<SqlValueNode> values = VisitList(node.Values, usedColumns);
        return new InNode(column, values);
    }

    public override SqlTreeNode VisitExists(ExistsNode node, ISet<ColumnNode> usedColumns)
    {
        SelectNode subSelect = TypedVisit(node.SubSelect, usedColumns);
        return new ExistsNode(subSelect);
    }

    public override SqlTreeNode VisitCount(CountNode node, ISet<ColumnNode> usedColumns)
    {
        SelectNode subSelect = TypedVisit(node.SubSelect, usedColumns);
        return new CountNode(subSelect);
    }

    public override SqlTreeNode VisitOrderBy(OrderByNode node, ISet<ColumnNode> usedColumns)
    {
        IReadOnlyList<OrderByTermNode> terms = VisitList(node.Terms, usedColumns);
        return new OrderByNode(terms);
    }

    public override SqlTreeNode VisitOrderByColumn(OrderByColumnNode node, ISet<ColumnNode> usedColumns)
    {
        ColumnNode column = TypedVisit(node.Column, usedColumns);
        return new OrderByColumnNode(column, node.IsAscending);
    }

    public override SqlTreeNode VisitOrderByCount(OrderByCountNode node, ISet<ColumnNode> usedColumns)
    {
        CountNode count = TypedVisit(node.Count, usedColumns);
        return new OrderByCountNode(count, node.IsAscending);
    }

    [return: NotNullIfNotNull("node")]
    private T? TypedVisit<T>(T? node, ISet<ColumnNode> usedColumns)
        where T : SqlTreeNode
    {
        return node != null ? (T)Visit(node, usedColumns) : null;
    }

    private IReadOnlyList<T> VisitList<T>(IEnumerable<T> nodes, ISet<ColumnNode> usedColumns)
        where T : SqlTreeNode
    {
        return nodes.Select(element => TypedVisit(element, usedColumns)).ToList();
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Started removal of unused selectors.")]
    private partial void LogStarted();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Finished removal of unused selectors.")]
    private partial void LogFinished();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Removing unused selector {Selector}.")]
    private partial void LogSelectorRemoved(ColumnSelectorNode selector);
}
