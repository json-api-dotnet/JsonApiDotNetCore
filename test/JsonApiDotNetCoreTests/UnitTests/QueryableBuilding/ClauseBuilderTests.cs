using System.ComponentModel.Design;
using System.Linq.Expressions;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryableBuilding;

public sealed class ClauseBuilderTests : IDisposable
{
    private readonly ResourceType _resourceType;
    private readonly IReadOnlyModel _entityModel;
    private readonly ServiceContainer _serviceContainer;
    private readonly IResourceFactory _resourceFactory;
    private readonly FilterParser _filterParser;
    private readonly SortParser _sortParser;

    public ClauseBuilderTests()
    {
        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<CompoundModel, long>().Build();

        _resourceType = resourceGraph.GetResourceType<CompoundModel>();

        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<CompoundModel>();
        _entityModel = modelBuilder.Model.FinalizeModel();

        _serviceContainer = new ServiceContainer();
        _resourceFactory = new ResourceFactory(_serviceContainer);
        _filterParser = new FilterParser(_resourceFactory);
        _sortParser = new SortParser();
    }

    [Fact]
    public void Builds_where_clause_for_nested_compound_attribute()
    {
        // Arrange
        FilterExpression filter = _filterParser.Parse("equals(details.title,'abc')", _resourceType);

        var whereClauseBuilder = new WhereClauseBuilder();
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = whereClauseBuilder.ApplyWhere(filter, context);

        // Assert
        result.ToString().Should().Contain(".Where(compoundModel => (compoundModel.Details.Title == value(");
    }

    [Fact]
    public void Builds_where_clause_for_null_comparison_on_compound_attribute()
    {
        // Arrange
        FilterExpression filter = _filterParser.Parse("equals(details,null)", _resourceType);

        var whereClauseBuilder = new WhereClauseBuilder();
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = whereClauseBuilder.ApplyWhere(filter, context);

        // Assert
        result.ToString().Should().Contain(".Where(compoundModel => (compoundModel.Details == Convert(null, CompoundRoot)))");
    }

    [Fact]
    public void Builds_where_clause_for_compound_collection_has()
    {
        // Arrange
        FilterExpression filter = _filterParser.Parse("has(details.items,equals(label,'work'))", _resourceType);

        var whereClauseBuilder = new WhereClauseBuilder();
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = whereClauseBuilder.ApplyWhere(filter, context);

        // Assert
        result.ToString().Should().Contain(".Where(compoundModel => compoundModel.Details.Items.Any(compoundItem => (compoundItem.Label == value(");
    }

    [Fact]
    public void Builds_where_clause_for_primitive_collection_has_with_element_symbol()
    {
        // Arrange
        FilterExpression filter = _filterParser.Parse("has(tags,equals(_,'urgent'))", _resourceType);

        var whereClauseBuilder = new WhereClauseBuilder();
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = whereClauseBuilder.ApplyWhere(filter, context);

        // Assert
        result.ToString().Should().Contain(".Where(compoundModel => compoundModel.Tags.Any(string => (string == value(");
    }

    [Fact]
    public void Builds_where_clause_for_primitive_collection_has_with_startsWith()
    {
        // Arrange
        FilterExpression filter = _filterParser.Parse("has(tags,startsWith(_,'urg'))", _resourceType);

        var whereClauseBuilder = new WhereClauseBuilder();
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = whereClauseBuilder.ApplyWhere(filter, context);

        // Assert
        result.ToString().Should().Contain(".Where(compoundModel => compoundModel.Tags.Any(string => string.StartsWith(value(");
    }

    [Fact]
    public void Builds_where_clause_for_collection_count()
    {
        // Arrange
        FilterExpression filter = _filterParser.Parse("greaterThan(count(details.items),'1')", _resourceType);

        var whereClauseBuilder = new WhereClauseBuilder();
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = whereClauseBuilder.ApplyWhere(filter, context);

        // Assert
        result.ToString().Should().Contain(".Where(compoundModel => (compoundModel.Details.Items.Count > value(");
    }

    [Fact]
    public void Builds_order_by_clause_for_nested_scalar()
    {
        // Arrange
        SortExpression sort = _sortParser.Parse("-details.nested.description,details.title", _resourceType);

        var orderClauseBuilder = new OrderClauseBuilder();
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = orderClauseBuilder.ApplyOrderBy(sort, context);

        // Assert
        result.ToString().Should().Contain(
            ".OrderByDescending(compoundModel => compoundModel.Details.Nested.Description).ThenBy(compoundModel => compoundModel.Details.Title)");
    }

    [Fact]
    public void Builds_select_clause_for_compound_attribute()
    {
        // Arrange
        var selection = new FieldSelection();
        FieldSelectors selectors = selection.GetOrCreateSelectors(_resourceType);
        AttrAttribute detailsAttr = _resourceType.GetAttributeByPropertyName(nameof(CompoundModel.Details));
        selectors.IncludeAttribute(detailsAttr);
        FieldSelectors childSelectors = selectors.GetOrCreateChildSelectors(detailsAttr);
        AttrAttribute titleAttr = detailsAttr.CompoundType!.GetAttributeByPropertyName(nameof(CompoundRoot.Title));
        childSelectors.IncludeAttribute(titleAttr);

        var selectClauseBuilder = new SelectClauseBuilder(_resourceFactory);
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = selectClauseBuilder.ApplySelect(selection, context);

        // Assert
        result.ToString().Should().Contain("new CompoundRoot() {Title = compoundModel.Details.Title}");
    }

    [Fact]
    public void Builds_select_clause_for_nested_compound_attribute()
    {
        // Arrange
        var selection = new FieldSelection();
        FieldSelectors selectors = selection.GetOrCreateSelectors(_resourceType);
        AttrAttribute detailsAttr = _resourceType.GetAttributeByPropertyName(nameof(CompoundModel.Details));
        selectors.IncludeAttribute(detailsAttr);
        FieldSelectors childSelectors = selectors.GetOrCreateChildSelectors(detailsAttr);
        AttrAttribute nestedAttr = detailsAttr.CompoundType!.GetAttributeByPropertyName(nameof(CompoundRoot.Nested));
        childSelectors.IncludeAttribute(nestedAttr);
        FieldSelectors grandchildSelectors = childSelectors.GetOrCreateChildSelectors(nestedAttr);
        AttrAttribute descAttr = nestedAttr.CompoundType!.GetAttributeByPropertyName(nameof(CompoundNested.Description));
        grandchildSelectors.IncludeAttribute(descAttr);

        var selectClauseBuilder = new SelectClauseBuilder(_resourceFactory);
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = selectClauseBuilder.ApplySelect(selection, context);

        // Assert
        result.ToString().Should().Contain("new CompoundNested() {Description = compoundModel.Details.Nested.Description}");
    }

    [Fact]
    public void Builds_select_clause_for_compound_collection_attribute_without_crashing()
    {
        // Arrange
        var selection = new FieldSelection();
        FieldSelectors selectors = selection.GetOrCreateSelectors(_resourceType);
        AttrAttribute itemsAttr = _resourceType.GetAttributeByPropertyName(nameof(CompoundModel.Items));
        selectors.IncludeAttribute(itemsAttr);
        FieldSelectors childSelectors = selectors.GetOrCreateChildSelectors(itemsAttr);
        AttrAttribute labelAttr = itemsAttr.CompoundType!.GetAttributeByPropertyName(nameof(CompoundItem.Label));
        childSelectors.IncludeAttribute(labelAttr);

        var selectClauseBuilder = new SelectClauseBuilder(_resourceFactory);
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(CompoundModel));

        IQueryable<CompoundModel> source = Array.Empty<CompoundModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, _resourceType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = selectClauseBuilder.ApplySelect(selection, context);

        // Assert
        result.ToString().Should().Contain("Items = compoundModel.Items");
    }

    public void Dispose()
    {
        _serviceContainer.Dispose();
    }

    private sealed class DummyQueryableBuilder : IQueryableBuilder
    {
        public Expression ApplyQuery(QueryLayer layer, QueryableBuilderContext context)
        {
            throw new NotImplementedException();
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class CompoundModel : Identifiable<long>
    {
        [Attr]
        public string? Name { get; set; }

        [Attr(IsCompound = true)]
        public CompoundRoot? Details { get; set; }

        [Attr]
        public List<string> Tags { get; set; } = [];

        [Attr(IsCompound = true)]
        public List<CompoundItem> Items { get; set; } = [];
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CompoundRoot
    {
        [Attr]
        public string? Title { get; set; }

        [Attr(IsCompound = true)]
        public CompoundNested? Nested { get; set; }

        [Attr]
        public List<int> Numbers { get; set; } = [];

        [Attr(IsCompound = true)]
        public List<CompoundItem> Items { get; set; } = [];
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CompoundNested
    {
        [Attr]
        public string? Description { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CompoundItem
    {
        [Attr]
        public string? Label { get; set; }
    }
}
