using System.Collections.ObjectModel;
using System.Net;
using DapperExample.TranslationToSql.DataModel;
using DapperExample.TranslationToSql.Generators;
using DapperExample.TranslationToSql.Transformations;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace DapperExample.TranslationToSql.Builders;

/// <summary>
/// Builds a SELECT statement from a <see cref="QueryLayer" />.
/// </summary>
internal sealed class SelectStatementBuilder : QueryExpressionVisitor<TableAccessorNode, SqlTreeNode>
{
    // State that is shared between sub-queries.
    private readonly QueryState _queryState;

    // The FROM/JOIN/sub-SELECT tables, along with their selectors (which usually are column references).
    private readonly Dictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> _selectorsPerTable = [];

    // Used to assign unique names when adding selectors, in case tables are joined that would result in duplicate column names.
    private readonly HashSet<string> _selectorNamesUsed = [];

    // Filter constraints.
    private readonly List<FilterNode> _whereFilters = [];

    // Sorting on columns, or COUNT(*) in a sub-query.
    private readonly List<OrderByTermNode> _orderByTerms = [];

    // Indicates whether to select a set of columns, the number of rows, or only the first (unnamed) column.
    private SelectShape _selectShape;

    public SelectStatementBuilder(IDataModelService dataModelService, ILoggerFactory loggerFactory)
        : this(new QueryState(dataModelService, new TableAliasGenerator(), new ParameterGenerator(), loggerFactory))
    {
    }

    private SelectStatementBuilder(QueryState queryState)
    {
        _queryState = queryState;
    }

    public SelectNode Build(QueryLayer queryLayer, SelectShape selectShape)
    {
        ArgumentGuard.NotNull(queryLayer);

        // Convert queryLayer.Include into multiple levels of queryLayer.Selection.
        var includeConverter = new QueryLayerIncludeConverter(queryLayer);
        includeConverter.ConvertIncludesToSelections();

        ResetState(selectShape);

        FromNode primaryTableAccessor = CreatePrimaryTable(queryLayer.ResourceType);
        ConvertQueryLayer(queryLayer, primaryTableAccessor);

        SelectNode select = ToSelect(false, false);

        if (_selectShape == SelectShape.Columns)
        {
            var staleRewriter = new StaleColumnReferenceRewriter(_queryState.OldToNewTableAliasMap, _queryState.LoggerFactory);
            select = staleRewriter.PullColumnsIntoScope(select);

            var selectorsRewriter = new UnusedSelectorsRewriter(_queryState.LoggerFactory);
            select = selectorsRewriter.RemoveUnusedSelectorsInSubQueries(select);
        }

        return select;
    }

    private void ResetState(SelectShape selectShape)
    {
        _queryState.Reset();
        _selectorsPerTable.Clear();
        _selectorNamesUsed.Clear();
        _whereFilters.Clear();
        _orderByTerms.Clear();
        _selectShape = selectShape;
    }

    private FromNode CreatePrimaryTable(ResourceType resourceType)
    {
        IReadOnlyDictionary<string, ResourceFieldAttribute?> columnMappings = _queryState.DataModelService.GetColumnMappings(resourceType);
        var table = new TableNode(resourceType, columnMappings, _queryState.TableAliasGenerator.GetNext());
        var from = new FromNode(table);

        TrackPrimaryTable(from);
        return from;
    }

    private void TrackPrimaryTable(TableAccessorNode tableAccessor)
    {
        if (_selectorsPerTable.Count > 0)
        {
            throw new InvalidOperationException("A primary table already exists.");
        }

        _queryState.RelatedTables.Add(tableAccessor, []);

        _selectorsPerTable[tableAccessor] = _selectShape switch
        {
            SelectShape.Columns => Array.Empty<SelectorNode>(),
            SelectShape.Count => [new CountSelectorNode(null)],
            _ => [new OneSelectorNode(null)]
        };
    }

    private void ConvertQueryLayer(QueryLayer queryLayer, TableAccessorNode tableAccessor)
    {
        if (queryLayer.Filter != null)
        {
            var filter = (FilterNode)Visit(queryLayer.Filter, tableAccessor);
            _whereFilters.Add(filter);
        }

        if (queryLayer.Sort != null)
        {
            var orderBy = (OrderByNode)Visit(queryLayer.Sort, tableAccessor);
            _orderByTerms.AddRange(orderBy.Terms);
        }

        if (queryLayer.Pagination is { PageSize: not null })
        {
            throw new NotSupportedException("Pagination is not supported.");
        }

        if (queryLayer.Selection != null)
        {
            foreach (ResourceType resourceType in queryLayer.Selection.GetResourceTypes())
            {
                FieldSelectors selectors = queryLayer.Selection.GetOrCreateSelectors(resourceType);
                ConvertFieldSelectors(selectors, tableAccessor);
            }
        }
    }

    private void ConvertFieldSelectors(FieldSelectors selectors, TableAccessorNode tableAccessor)
    {
        HashSet<ColumnNode> selectedColumns = [];
        Dictionary<RelationshipAttribute, QueryLayer> nextLayers = [];

        if (selectors.IsEmpty || selectors.ContainsReadOnlyAttribute || selectors.ContainsOnlyRelationships)
        {
            // If a read-only attribute is selected, its calculated value likely depends on another property, so fetch all scalar properties.
            // And only selecting relationships implicitly means to fetch all scalar properties as well.
            // Additionally, empty selectors (originating from eliminated includes) indicate to fetch all scalar properties too.

            selectedColumns = tableAccessor.Source.Columns.Where(column => column.Type == ColumnType.Scalar).ToHashSet();
        }

        foreach ((ResourceFieldAttribute field, QueryLayer? nextLayer) in selectors.OrderBy(selector => selector.Key.PublicName))
        {
            if (field is AttrAttribute attribute)
            {
                // Returns null when the set contains an unmapped column, which is silently ignored.
                ColumnNode? column = tableAccessor.Source.FindColumn(attribute.Property.Name, ColumnType.Scalar, tableAccessor.Source.Alias);

                if (column != null)
                {
                    selectedColumns.Add(column);
                }
            }

            if (field is RelationshipAttribute relationship && nextLayer != null)
            {
                nextLayers.Add(relationship, nextLayer);
            }
        }

        if (_selectShape == SelectShape.Columns)
        {
            SetColumnSelectors(tableAccessor, selectedColumns);
        }

        foreach ((RelationshipAttribute relationship, QueryLayer nextLayer) in nextLayers)
        {
            ConvertNestedQueryLayer(tableAccessor, relationship, nextLayer);
        }
    }

    private void SetColumnSelectors(TableAccessorNode tableAccessor, IEnumerable<ColumnNode> columns)
    {
        if (!_selectorsPerTable.ContainsKey(tableAccessor))
        {
            throw new InvalidOperationException($"Table {tableAccessor.Source.Alias} not found in selected tables.");
        }

        // When selecting from a table, use a deterministic order to simplify test assertions.
        // When selecting from a sub-query (typically spanning multiple tables and renamed columns), existing order must be preserved.
        _selectorsPerTable[tableAccessor] = tableAccessor.Source is SelectNode
            ? PreserveColumnOrderEnsuringUniqueNames(columns).AsReadOnly()
            : OrderColumnsWithIdAtFrontEnsuringUniqueNames(columns).AsReadOnly();
    }

    private List<SelectorNode> PreserveColumnOrderEnsuringUniqueNames(IEnumerable<ColumnNode> columns)
    {
        List<SelectorNode> selectors = [];

        foreach (ColumnNode column in columns)
        {
            string uniqueName = GetUniqueSelectorName(column.Name);
            string? selectorAlias = uniqueName != column.Name ? uniqueName : null;
            var columnSelector = new ColumnSelectorNode(column, selectorAlias);
            selectors.Add(columnSelector);
        }

        return selectors;
    }

    private SelectorNode[] OrderColumnsWithIdAtFrontEnsuringUniqueNames(IEnumerable<ColumnNode> columns)
    {
        Dictionary<string, List<SelectorNode>> selectorsPerTable = [];

        foreach (ColumnNode column in columns.OrderBy(column => column.GetTableAliasIndex()).ThenBy(column => column.Name))
        {
            string tableAlias = column.TableAlias ?? "!";
            selectorsPerTable.TryAdd(tableAlias, []);

            string uniqueName = GetUniqueSelectorName(column.Name);
            string? selectorAlias = uniqueName != column.Name ? uniqueName : null;
            var columnSelector = new ColumnSelectorNode(column, selectorAlias);

            if (column.Name == TableSourceNode.IdColumnName)
            {
                selectorsPerTable[tableAlias].Insert(0, columnSelector);
            }
            else
            {
                selectorsPerTable[tableAlias].Add(columnSelector);
            }
        }

        return selectorsPerTable.SelectMany(selector => selector.Value).ToArray();
    }

    private string GetUniqueSelectorName(string columnName)
    {
        string uniqueName = columnName;

        while (_selectorNamesUsed.Contains(uniqueName))
        {
            uniqueName += "0";
        }

        _selectorNamesUsed.Add(uniqueName);
        return uniqueName;
    }

    private void ConvertNestedQueryLayer(TableAccessorNode tableAccessor, RelationshipAttribute relationship, QueryLayer nextLayer)
    {
        bool requireSubQuery = nextLayer.Filter != null;

        if (requireSubQuery)
        {
            var subSelectBuilder = new SelectStatementBuilder(_queryState);

            FromNode primaryTableAccessor = subSelectBuilder.CreatePrimaryTable(relationship.RightType);
            subSelectBuilder.ConvertQueryLayer(nextLayer, primaryTableAccessor);

            string[] innerTableAliases = subSelectBuilder._selectorsPerTable.Keys.Select(accessor => accessor.Source.Alias).Cast<string>().ToArray();

            // In the sub-query, select all columns, to enable referencing them from other locations in the query.
            // This usually produces unused selectors, which will be removed in a post-processing step.
            var selectorsToKeep = new Dictionary<TableAccessorNode, IReadOnlyList<SelectorNode>>(subSelectBuilder._selectorsPerTable);
            subSelectBuilder.SelectAllColumnsInAllTables(selectorsToKeep.Keys);

            // Since there's no pagination support, it's pointless to preserve orderings in the sub-query.
            OrderByTermNode[] orderingsToKeep = subSelectBuilder._orderByTerms.ToArray();
            subSelectBuilder._orderByTerms.Clear();

            SelectNode aliasedSubQuery = subSelectBuilder.ToSelect(true, true);

            // Store inner-to-outer table aliases, to enable rewriting stale column references in a post-processing step.
            // This is required for orderings that contain sub-selects, resulting from order-by-count.
            MapOldTableAliasesToSubQuery(innerTableAliases, aliasedSubQuery.Alias!);

            TableAccessorNode outerTableAccessor = CreateRelatedTable(tableAccessor, relationship, aliasedSubQuery);

            // In the outer query, select only what was originally selected.
            _selectorsPerTable[outerTableAccessor] =
                MapSelectorsFromSubQuery(selectorsToKeep.SelectMany(selector => selector.Value), aliasedSubQuery).AsReadOnly();

            // To achieve total ordering, all orderings from sub-query must always appear in the root query.
            List<OrderByTermNode> outerOrderingsToAdd = MapOrderingsFromSubQuery(orderingsToKeep, aliasedSubQuery);
            _orderByTerms.AddRange(outerOrderingsToAdd);
        }
        else
        {
            TableAccessorNode relatedTableAccessor = GetOrCreateRelatedTable(tableAccessor, relationship);
            ConvertQueryLayer(nextLayer, relatedTableAccessor);
        }
    }

    private void SelectAllColumnsInAllTables(IEnumerable<TableAccessorNode> tableAccessors)
    {
        _selectorsPerTable.Clear();
        _selectorNamesUsed.Clear();

        foreach (TableAccessorNode tableAccessor in tableAccessors)
        {
            _selectorsPerTable.Add(tableAccessor, Array.Empty<SelectorNode>());

            if (_selectShape == SelectShape.Columns)
            {
                SetColumnSelectors(tableAccessor, tableAccessor.Source.Columns);
            }
        }
    }

    private void MapOldTableAliasesToSubQuery(IEnumerable<string> oldTableAliases, string newTableAlias)
    {
        foreach (string oldTableAlias in oldTableAliases)
        {
            _queryState.OldToNewTableAliasMap[oldTableAlias] = newTableAlias;
        }
    }

    private TableAccessorNode CreateRelatedTable(TableAccessorNode leftTableAccessor, RelationshipAttribute relationship, TableSourceNode rightTableSource)
    {
        RelationshipForeignKey foreignKey = _queryState.DataModelService.GetForeignKey(relationship);
        JoinType joinType = foreignKey is { IsAtLeftSide: true, IsNullable: false } ? JoinType.InnerJoin : JoinType.LeftJoin;

        ComparisonNode joinCondition = CreateJoinCondition(leftTableAccessor.Source, relationship, rightTableSource);

        TableAccessorNode relatedTableAccessor = new JoinNode(joinType, rightTableSource, (ColumnNode)joinCondition.Left, (ColumnNode)joinCondition.Right);

        TrackRelatedTable(leftTableAccessor, relationship, relatedTableAccessor);
        return relatedTableAccessor;
    }

    private ComparisonNode CreateJoinCondition(TableSourceNode outerTableSource, RelationshipAttribute relationship, TableSourceNode innerTableSource)
    {
        RelationshipForeignKey foreignKey = _queryState.DataModelService.GetForeignKey(relationship);

        ColumnNode innerColumn = foreignKey.IsAtLeftSide
            ? innerTableSource.GetIdColumn(innerTableSource.Alias)
            : innerTableSource.GetColumn(foreignKey.ColumnName, ColumnType.ForeignKey, innerTableSource.Alias);

        ColumnNode outerColumn = foreignKey.IsAtLeftSide
            ? outerTableSource.GetColumn(foreignKey.ColumnName, ColumnType.ForeignKey, outerTableSource.Alias)
            : outerTableSource.GetIdColumn(outerTableSource.Alias);

        return new ComparisonNode(ComparisonOperator.Equals, outerColumn, innerColumn);
    }

    private void TrackRelatedTable(TableAccessorNode leftTableAccessor, RelationshipAttribute relationship, TableAccessorNode rightTableAccessor)
    {
        _queryState.RelatedTables.Add(rightTableAccessor, []);
        _selectorsPerTable[rightTableAccessor] = Array.Empty<SelectorNode>();

        _queryState.RelatedTables[leftTableAccessor].Add(relationship, rightTableAccessor);
    }

    private List<SelectorNode> MapSelectorsFromSubQuery(IEnumerable<SelectorNode> innerSelectorsToKeep, SelectNode select)
    {
        List<ColumnNode> outerColumnsToKeep = [];

        foreach (SelectorNode innerSelector in innerSelectorsToKeep)
        {
            if (innerSelector is ColumnSelectorNode innerColumnSelector)
            {
                // t2."Id" AS Id0 => t3.Id0
                ColumnNode innerColumn = innerColumnSelector.Column;
                ColumnNode outerColumn = select.Columns.Single(outerColumn => outerColumn.Selector.Column == innerColumn);
                outerColumnsToKeep.Add(outerColumn);
            }
            else
            {
                // If there's an alias, we should use it. Otherwise we could fallback to ordinal selector.
                throw new NotImplementedException("Mapping non-column selectors is not implemented.");
            }
        }

        return PreserveColumnOrderEnsuringUniqueNames(outerColumnsToKeep);
    }

    private List<OrderByTermNode> MapOrderingsFromSubQuery(IEnumerable<OrderByTermNode> innerOrderingsToKeep, SelectNode select)
    {
        List<OrderByTermNode> orderingsToKeep = [];

        foreach (OrderByTermNode innerTerm in innerOrderingsToKeep)
        {
            if (innerTerm is OrderByColumnNode orderByColumn)
            {
                ColumnNode outerColumn = select.Columns.Single(outerColumn => outerColumn.Selector.Column == orderByColumn.Column);
                var outerTerm = new OrderByColumnNode(outerColumn, innerTerm.IsAscending);
                orderingsToKeep.Add(outerTerm);
            }
            else
            {
                // Rewriting stale column references from order-by-count is non-trivial, so let the post-processor handle them.
                orderingsToKeep.Add(innerTerm);
            }
        }

        return orderingsToKeep;
    }

    private TableAccessorNode GetOrCreateRelatedTable(TableAccessorNode leftTableAccessor, RelationshipAttribute relationship)
    {
        TableAccessorNode? relatedTableAccessor = _selectorsPerTable.Count == 0
            // Joining against something in an outer query.
            ? CreatePrimaryTableWithIdentityCondition(leftTableAccessor.Source, relationship)
            : FindRelatedTable(leftTableAccessor, relationship);

        if (relatedTableAccessor == null)
        {
            IReadOnlyDictionary<string, ResourceFieldAttribute?> columnMappings = _queryState.DataModelService.GetColumnMappings(relationship.RightType);
            var rightTable = new TableNode(relationship.RightType, columnMappings, _queryState.TableAliasGenerator.GetNext());

            return CreateRelatedTable(leftTableAccessor, relationship, rightTable);
        }

        return relatedTableAccessor;
    }

    private FromNode CreatePrimaryTableWithIdentityCondition(TableSourceNode outerTableSource, RelationshipAttribute relationship)
    {
        FromNode innerTableAccessor = CreatePrimaryTable(relationship.RightType);

        ComparisonNode joinCondition = CreateJoinCondition(outerTableSource, relationship, innerTableAccessor.Source);
        _whereFilters.Add(joinCondition);

        return innerTableAccessor;
    }

    private TableAccessorNode? FindRelatedTable(TableAccessorNode leftTableAccessor, RelationshipAttribute relationship)
    {
        Dictionary<RelationshipAttribute, TableAccessorNode> rightTableAccessors = _queryState.RelatedTables[leftTableAccessor];
        return rightTableAccessors.GetValueOrDefault(relationship);
    }

    private SelectNode ToSelect(bool isSubQuery, bool createAlias)
    {
        WhereNode? where = GetWhere();
        OrderByNode? orderBy = _orderByTerms.Count == 0 ? null : new OrderByNode(_orderByTerms.AsReadOnly());

        // Materialization using Dapper requires selectors to match property names, so adjust selector names accordingly.
        Dictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> selectorsPerTable =
            isSubQuery ? _selectorsPerTable : AliasSelectorsToTableColumnNames(_selectorsPerTable);

        string? alias = createAlias ? _queryState.TableAliasGenerator.GetNext() : null;
        return new SelectNode(selectorsPerTable.AsReadOnly(), where, orderBy, alias);
    }

    private WhereNode? GetWhere()
    {
        if (_whereFilters.Count == 0)
        {
            return null;
        }

        var combinator = new LogicalCombinator();

        FilterNode filter = _whereFilters.Count == 1 ? _whereFilters[0] : new LogicalNode(LogicalOperator.And, _whereFilters.AsReadOnly());
        FilterNode collapsed = combinator.Collapse(filter);

        return new WhereNode(collapsed);
    }

    private static Dictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> AliasSelectorsToTableColumnNames(
        Dictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> selectorsPerTable)
    {
        Dictionary<TableAccessorNode, IReadOnlyList<SelectorNode>> aliasedSelectors = [];

        foreach ((TableAccessorNode tableAccessor, IReadOnlyList<SelectorNode> tableSelectors) in selectorsPerTable)
        {
            aliasedSelectors[tableAccessor] = tableSelectors.Select(AliasToTableColumnName).ToArray().AsReadOnly();
        }

        return aliasedSelectors;
    }

    private static SelectorNode AliasToTableColumnName(SelectorNode selector)
    {
        if (selector is ColumnSelectorNode columnSelector)
        {
            if (columnSelector.Column is ColumnInSelectNode columnInSelect)
            {
                string persistedColumnName = columnInSelect.GetPersistedColumnName();

                if (columnInSelect.Name != persistedColumnName)
                {
                    // t1.Id0 => t1.Id0 AS Id
                    return new ColumnSelectorNode(columnInSelect, persistedColumnName);
                }
            }

            if (columnSelector.Alias != null)
            {
                // t1."Id" AS Id0 => t1."Id"
                return new ColumnSelectorNode(columnSelector.Column, null);
            }
        }

        return selector;
    }

    public override SqlTreeNode DefaultVisit(QueryExpression expression, TableAccessorNode tableAccessor)
    {
        throw new NotSupportedException($"Expressions of type '{expression.GetType().Name}' are not supported.");
    }

    public override SqlTreeNode VisitComparison(ComparisonExpression expression, TableAccessorNode tableAccessor)
    {
        SqlValueNode left = VisitComparisonTerm(expression.Left, tableAccessor);
        SqlValueNode right = VisitComparisonTerm(expression.Right, tableAccessor);

        return new ComparisonNode(expression.Operator, left, right);
    }

    private SqlValueNode VisitComparisonTerm(QueryExpression comparisonTerm, TableAccessorNode tableAccessor)
    {
        if (comparisonTerm is NullConstantExpression)
        {
            return NullConstantNode.Instance;
        }

        SqlTreeNode treeNode = Visit(comparisonTerm, tableAccessor);

        if (treeNode is JoinNode join)
        {
            return join.InnerColumn;
        }

        return (SqlValueNode)treeNode;
    }

    public override SqlTreeNode VisitResourceFieldChain(ResourceFieldChainExpression expression, TableAccessorNode tableAccessor)
    {
        TableAccessorNode currentAccessor = tableAccessor;

        foreach (ResourceFieldAttribute field in expression.Fields)
        {
            if (field is RelationshipAttribute relationship)
            {
                currentAccessor = GetOrCreateRelatedTable(currentAccessor, relationship);
            }
            else if (field is AttrAttribute attribute)
            {
                ColumnNode? column = currentAccessor.Source.FindColumn(attribute.Property.Name, ColumnType.Scalar, currentAccessor.Source.Alias);

                if (column == null)
                {
                    // Unmapped columns cannot be translated to SQL.
                    throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
                    {
                        Title = "Sorting or filtering on the requested attribute is unavailable.",
                        Detail = $"Sorting or filtering on attribute '{attribute.PublicName}' is unavailable because it is unmapped."
                    });
                }

                return column;
            }
        }

        return currentAccessor;
    }

    public override SqlTreeNode VisitLiteralConstant(LiteralConstantExpression expression, TableAccessorNode tableAccessor)
    {
        return _queryState.ParameterGenerator.Create(expression.TypedValue);
    }

    public override SqlTreeNode VisitLogical(LogicalExpression expression, TableAccessorNode tableAccessor)
    {
        ReadOnlyCollection<FilterNode> terms = VisitSequence<FilterExpression, FilterNode>(expression.Terms, tableAccessor);
        return new LogicalNode(expression.Operator, terms);
    }

    private ReadOnlyCollection<TOut> VisitSequence<TIn, TOut>(IEnumerable<TIn> source, TableAccessorNode tableAccessor)
        where TIn : QueryExpression
        where TOut : SqlTreeNode
    {
        return source.Select(expression => (TOut)Visit(expression, tableAccessor)).ToArray().AsReadOnly();
    }

    public override SqlTreeNode VisitNot(NotExpression expression, TableAccessorNode tableAccessor)
    {
        var child = (FilterNode)Visit(expression.Child, tableAccessor);
        FilterNode filter = child is NotNode notChild ? notChild.Child : new NotNode(child);

        var finder = new NullableAttributeFinder(_queryState.DataModelService);
        finder.Visit(expression, null);

        if (finder.AttributesToNullCheck.Count > 0)
        {
            List<FilterNode> orTerms = [filter];

            foreach (ResourceFieldChainExpression fieldChain in finder.AttributesToNullCheck)
            {
                var column = (ColumnInTableNode)Visit(fieldChain, tableAccessor);
                var isNullCheck = new ComparisonNode(ComparisonOperator.Equals, column, NullConstantNode.Instance);
                orTerms.Add(isNullCheck);
            }

            return new LogicalNode(LogicalOperator.Or, orTerms.AsReadOnly());
        }

        return filter;
    }

    public override SqlTreeNode VisitHas(HasExpression expression, TableAccessorNode tableAccessor)
    {
        var subSelectBuilder = new SelectStatementBuilder(_queryState)
        {
            _selectShape = SelectShape.One
        };

        return subSelectBuilder.GetExistsClause(expression, tableAccessor);
    }

    private ExistsNode GetExistsClause(HasExpression expression, TableAccessorNode outerTableAccessor)
    {
        var rightTableAccessor = (TableAccessorNode)Visit(expression.TargetCollection, outerTableAccessor);

        if (expression.Filter != null)
        {
            var filter = (FilterNode)Visit(expression.Filter, rightTableAccessor);
            _whereFilters.Add(filter);
        }

        SelectNode select = ToSelect(true, false);
        return new ExistsNode(select);
    }

    public override SqlTreeNode VisitIsType(IsTypeExpression expression, TableAccessorNode tableAccessor)
    {
        throw new NotSupportedException("Resource inheritance is not supported.");
    }

    public override SqlTreeNode VisitSortElement(SortElementExpression expression, TableAccessorNode tableAccessor)
    {
        if (expression.Target is CountExpression count)
        {
            var newCount = (CountNode)Visit(count, tableAccessor);
            return new OrderByCountNode(newCount, expression.IsAscending);
        }

        if (expression.Target is ResourceFieldChainExpression fieldChain)
        {
            var column = (ColumnNode)Visit(fieldChain, tableAccessor);
            return new OrderByColumnNode(column, expression.IsAscending);
        }

        throw new NotSupportedException($"Unsupported sort type '{expression.Target.GetType().Name}' with value '{expression.Target}'.");
    }

    public override SqlTreeNode VisitSort(SortExpression expression, TableAccessorNode tableAccessor)
    {
        ReadOnlyCollection<OrderByTermNode> terms = VisitSequence<SortElementExpression, OrderByTermNode>(expression.Elements, tableAccessor);
        return new OrderByNode(terms);
    }

    public override SqlTreeNode VisitCount(CountExpression expression, TableAccessorNode tableAccessor)
    {
        var subSelectBuilder = new SelectStatementBuilder(_queryState)
        {
            _selectShape = SelectShape.Count
        };

        return subSelectBuilder.GetCountClause(expression, tableAccessor);
    }

    private CountNode GetCountClause(CountExpression expression, TableAccessorNode outerTableAccessor)
    {
        _ = Visit(expression.TargetCollection, outerTableAccessor);

        SelectNode select = ToSelect(true, false);
        return new CountNode(select);
    }

    public override SqlTreeNode VisitMatchText(MatchTextExpression expression, TableAccessorNode tableAccessor)
    {
        var column = (ColumnNode)Visit(expression.TargetAttribute, tableAccessor);
        return new LikeNode(column, expression.MatchKind, (string)expression.TextValue.TypedValue);
    }

    public override SqlTreeNode VisitAny(AnyExpression expression, TableAccessorNode tableAccessor)
    {
        var column = (ColumnNode)Visit(expression.TargetAttribute, tableAccessor);

        ReadOnlyCollection<ParameterNode> parameters =
            VisitSequence<LiteralConstantExpression, ParameterNode>(expression.Constants.OrderBy(constant => constant.TypedValue), tableAccessor);

        return parameters.Count == 1 ? new ComparisonNode(ComparisonOperator.Equals, column, parameters[0]) : new InNode(column, parameters);
    }

    private sealed class NullableAttributeFinder : QueryExpressionRewriter<object?>
    {
        private readonly IDataModelService _dataModelService;

        public List<ResourceFieldChainExpression> AttributesToNullCheck { get; } = [];

        public NullableAttributeFinder(IDataModelService dataModelService)
        {
            ArgumentGuard.NotNull(dataModelService);

            _dataModelService = dataModelService;
        }

        public override QueryExpression VisitResourceFieldChain(ResourceFieldChainExpression expression, object? argument)
        {
            bool seenOptionalToOneRelationship = false;

            foreach (ResourceFieldAttribute field in expression.Fields)
            {
                if (field is HasOneAttribute hasOneRelationship)
                {
                    RelationshipForeignKey foreignKey = _dataModelService.GetForeignKey(hasOneRelationship);

                    if (foreignKey.IsNullable)
                    {
                        seenOptionalToOneRelationship = true;
                    }
                }
                else if (field is AttrAttribute attribute)
                {
                    if (seenOptionalToOneRelationship || _dataModelService.IsColumnNullable(attribute))
                    {
                        AttributesToNullCheck.Add(expression);
                    }
                }
            }

            return base.VisitResourceFieldChain(expression, argument);
        }
    }

    private sealed class QueryState
    {
        // Provides access to the underlying data model (tables, columns and foreign keys).
        public IDataModelService DataModelService { get; }

        // Used to generate unique aliases for tables.
        public TableAliasGenerator TableAliasGenerator { get; }

        // Used to generate unique parameters for constants (to improve query plan caching and guard against SQL injection).
        public ParameterGenerator ParameterGenerator { get; }

        public ILoggerFactory LoggerFactory { get; }

        // Prevents importing a table multiple times and enables to reference a table imported by an inner/outer query.
        // In case of sub-queries, this may include temporary tables that won't survive in the final query.
        public Dictionary<TableAccessorNode, Dictionary<RelationshipAttribute, TableAccessorNode>> RelatedTables { get; } = [];

        // In case of sub-queries, we track old/new table aliases, so we can rewrite stale references afterwards.
        // This cannot be done in the moment itself, because references to tables are on method call stacks.
        public Dictionary<string, string> OldToNewTableAliasMap { get; } = [];

        public QueryState(IDataModelService dataModelService, TableAliasGenerator tableAliasGenerator, ParameterGenerator parameterGenerator,
            ILoggerFactory loggerFactory)
        {
            ArgumentGuard.NotNull(dataModelService);
            ArgumentGuard.NotNull(tableAliasGenerator);
            ArgumentGuard.NotNull(parameterGenerator);
            ArgumentGuard.NotNull(loggerFactory);

            DataModelService = dataModelService;
            TableAliasGenerator = tableAliasGenerator;
            ParameterGenerator = parameterGenerator;
            LoggerFactory = loggerFactory;
        }

        public void Reset()
        {
            TableAliasGenerator.Reset();
            ParameterGenerator.Reset();

            RelatedTables.Clear();
            OldToNewTableAliasMap.Clear();
        }
    }
}
