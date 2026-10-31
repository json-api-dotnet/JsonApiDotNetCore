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

public sealed class CompoundClauseBuilderTests : IDisposable
{
    private readonly IResourceGraph _resourceGraph;
    private readonly ResourceType _contactType;
    private readonly IReadOnlyModel _entityModel;
    private readonly ServiceContainer _serviceContainer;
    private readonly IResourceFactory _resourceFactory;
    private readonly FilterParser _filterParser;
    private readonly SortParser _sortParser;

    public CompoundClauseBuilderTests()
    {
        _resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
            .Add<ContactModel, long>()
            .Add<PlainModel, long>()
            .Add<ParentOfOneModel, long>()
            .Add<ParentOfManyModel, long>()
            .Add<ChildModel, long>()
            .Add<ListingModel, long>()
            .Add<ApartmentModel, long>()
            .Build();

        _contactType = _resourceGraph.GetResourceType<ContactModel>();

        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<ContactModel>();
        _entityModel = modelBuilder.Model.FinalizeModel();

        _serviceContainer = new ServiceContainer();
        _resourceFactory = new ResourceFactory(_serviceContainer);
        _filterParser = new FilterParser(_resourceFactory);
        _sortParser = new SortParser();
    }

    [Fact]
    public void Where_on_nested_compound_attribute_builds_member_chain()
    {
        // Act
        string text = BuildWhere("equals(content.name.lastName,'x')");

        // Assert
        text.Should().Contain(".Where(contactModel => (contactModel.Content.Name.LastName == value(");
    }

    [Fact]
    public void Has_without_predicate_builds_Any_without_predicate()
    {
        // Act
        string text = BuildWhere("has(phoneNumbers)");

        // Assert
        text.Should().Contain(".Where(contactModel => contactModel.PhoneNumbers.Any())");
    }

    [Fact]
    public void Has_with_predicate_on_compound_collection_builds_Any_with_predicate()
    {
        // Act
        string text = BuildWhere("has(phoneNumbers,equals(label,'x'))");

        // Assert
        text.Should().Contain(".Where(contactModel => contactModel.PhoneNumbers.Any(phoneNumber => (phoneNumber.Label == value(");
    }

    [Fact]
    public void Has_with_predicate_on_nested_compound_collection_builds_Any_with_predicate()
    {
        // Act
        string text = BuildWhere("has(content.phoneNumbers,equals(label,'x'))");

        // Assert
        text.Should().Contain(".Where(contactModel => contactModel.Content.PhoneNumbers.Any(phoneNumber => (phoneNumber.Label == value(");
    }

    [Fact]
    public void Has_with_element_symbol_on_scalar_collection_builds_Any_over_element()
    {
        // Act
        string text = BuildWhere("has(tags,equals(_,'x'))");

        // Assert
        text.Should().Contain(".Where(contactModel => contactModel.Tags.Any(string => (string == value(");
    }

    [Theory]
    [InlineData("stringList", "(string == Convert(null, String))")]
    [InlineData("nullableIntList", "(nullable`1 == Convert(null, Nullable`1))")]
    [InlineData("guidList", "(Convert(guid, Nullable`1) == Convert(null, Nullable`1))")]
    [InlineData("nullableGuidList", "(nullable`1 == Convert(null, Nullable`1))")]
    [InlineData("timeSpanList", "(Convert(timeSpan, Nullable`1) == Convert(null, Nullable`1))")]
    public void Has_with_null_element_comparison_coerces_constant_to_nullable(string collectionName, string expectedComparison)
    {
        // Act
        string text = BuildWhere($"has({collectionName},equals(_,null))");

        // Assert
        text.Should().Contain(expectedComparison);
    }

    [Fact]
    public void Count_on_list_uses_Count_property()
    {
        // Act
        string text = BuildWhere("greaterThan(count(tags),'1')");

        // Assert
        text.Should().Contain("(contactModel.Tags.Count > value(");
    }

    [Fact]
    public void Count_on_array_uses_Length_property()
    {
        // Act
        string text = BuildWhere("greaterThan(count(scores),'1')");

        // Assert
        text.Should().Contain("(contactModel.Scores.Length > value(");
    }

    [Fact]
    public void Sort_by_nested_scalar_and_by_count_builds_OrderBy_chain()
    {
        // Arrange
        SortExpression sort = _sortParser.Parse("content.name.lastName,-count(tags),-count(content.phoneNumbers)", _contactType);

        // Act
        string text = BuildOrderBy(sort);

        // Assert
        text.Should().Contain(".OrderBy(contactModel => contactModel.Content.Name.LastName)" +
            ".ThenByDescending(contactModel => contactModel.Tags.Count)" + ".ThenByDescending(contactModel => contactModel.Content.PhoneNumbers.Count)");
    }

    [Fact]
    public void Select_of_partial_compound_builds_null_guarded_nested_member_init()
    {
        // Arrange
        FieldSelection selection = CreateSelection(content => content.Include(nameof(ContactRoot.Name), name => name.Include(nameof(PersonName.LastName))));

        // Act
        string text = BuildSelect(selection);

        // Assert
        text.Should().Contain("Content = IIF((contactModel.Content == null), Convert(null, ContactRoot), new ContactRoot() {Name = IIF((contactModel.Content.Name == null), " +
            "Convert(null, PersonName), new PersonName() {LastName = contactModel.Content.Name.LastName})})");
    }

    [Fact]
    public void Select_of_getter_only_child_binds_all_writable_properties_of_compound()
    {
        // Arrange
        FieldSelection selection = CreateSelection(content => content.Include(nameof(ContactRoot.Calculated)));

        // Act
        string text = BuildSelect(selection);

        // Assert
        text.Should().Contain("Title = contactModel.Content.Title");
        text.Should().Contain("Tags = contactModel.Content.Tags");
        text.Should().Contain("PhoneNumbers = contactModel.Content.PhoneNumbers");
        text.Should().Contain("Name = contactModel.Content.Name");
        text.Should().NotContain("Calculated =");
    }

    [Fact]
    public void Select_of_collection_attribute_inside_compound_copies_collection_whole()
    {
        // Arrange
        FieldSelection selection = CreateSelection(content => content.Include(nameof(ContactRoot.PhoneNumbers),
            phoneNumbers => phoneNumbers.Include(nameof(PhoneNumber.Label))));

        // Act
        string text = BuildSelect(selection);

        // Assert
        text.Should().Contain("PhoneNumbers = contactModel.Content.PhoneNumbers");
        text.Should().NotContain("new PhoneNumber()");
    }

    [Fact]
    public void Include_of_relationship_on_type_with_compound_keeps_include_path_unchanged()
    {
        // Arrange
        ResourceType parentType = _resourceGraph.GetResourceType<ParentOfManyModel>();
        RelationshipAttribute childrenRelationship = parentType.GetRelationshipByPropertyName(nameof(ParentOfManyModel.Children));
        var include = new IncludeExpression([new IncludeElementExpression(childrenRelationship)]);

        var includeClauseBuilder = new IncludeClauseBuilder();
        var lambdaScopeFactory = new LambdaScopeFactory();
        using LambdaScope lambdaScope = lambdaScopeFactory.CreateScope(typeof(ParentOfManyModel));

        IQueryable<ParentOfManyModel> source = Array.Empty<ParentOfManyModel>().AsQueryable();

        var context = new QueryClauseBuilderContext(source.Expression, parentType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);

        // Act
        Expression result = includeClauseBuilder.ApplyInclude(include, context);

        // Assert
        result.ToString().Should().Contain(".Include(\"Children\")");
    }

    [Theory]
    [InlineData(BypassCase.CompoundOnTop, false)]
    [InlineData(BypassCase.CompoundOnChildToOne, true)]
    [InlineData(BypassCase.CompoundOnChildToMany, true)]
    [InlineData(BypassCase.CompoundOnDerivedTypeInSelection, false)]
    [InlineData(BypassCase.NoCompound, true)]
    public void ApplySelection_skips_select_only_when_selection_has_type_with_compound(BypassCase bypassCase, bool expectedSelectApplied)
    {
        // Arrange
        (ResourceType resourceType, FieldSelection selection) = CreateBypassSelection(bypassCase);

        // Act
        (bool selectApplied, bool sourceReturnedUnchanged) = ApplySelection(resourceType, selection);

        // Assert
        selectApplied.Should().Be(expectedSelectApplied);
        sourceReturnedUnchanged.Should().Be(!expectedSelectApplied);
    }

    [Fact(Skip = "KNOWN-ISSUE D15: A derived type with compound attributes that is absent from the selection does not cause the Select to be skipped")]
    public void ApplySelection_skips_select_when_only_derived_type_not_in_selection_has_compound()
    {
        // Arrange
        (ResourceType resourceType, FieldSelection selection) = CreateBypassSelection(BypassCase.CompoundOnDerivedTypeNotInSelection);

        // Act
        (bool selectApplied, bool sourceReturnedUnchanged) = ApplySelection(resourceType, selection);

        // Assert
        selectApplied.Should().BeFalse();
        sourceReturnedUnchanged.Should().BeTrue();
    }

    private (ResourceType ResourceType, FieldSelection Selection) CreateBypassSelection(BypassCase bypassCase)
    {
        switch (bypassCase)
        {
            case BypassCase.CompoundOnTop:
            {
                ResourceType type = _contactType;
                FieldSelection selection = new();
                selection.GetOrCreateSelectors(type).IncludeAttribute(type.GetAttributeByPropertyName(nameof(ContactModel.Name)));
                return (type, selection);
            }
            case BypassCase.CompoundOnChildToOne:
            {
                ResourceType type = _resourceGraph.GetResourceType<ParentOfOneModel>();
                ResourceType childType = _resourceGraph.GetResourceType<ChildModel>();
                return (type, CreateParentSelection(type, childType, nameof(ParentOfOneModel.Child)));
            }
            case BypassCase.CompoundOnChildToMany:
            {
                ResourceType type = _resourceGraph.GetResourceType<ParentOfManyModel>();
                ResourceType childType = _resourceGraph.GetResourceType<ChildModel>();
                return (type, CreateParentSelection(type, childType, nameof(ParentOfManyModel.Children)));
            }
            case BypassCase.CompoundOnDerivedTypeInSelection:
            {
                ResourceType type = _resourceGraph.GetResourceType<ListingModel>();
                ResourceType derivedType = _resourceGraph.GetResourceType<ApartmentModel>();
                FieldSelection selection = new();
                selection.GetOrCreateSelectors(type).IncludeAttribute(type.GetAttributeByPropertyName(nameof(ListingModel.Title)));
                selection.GetOrCreateSelectors(derivedType).IncludeAttribute(derivedType.GetAttributeByPropertyName(nameof(ApartmentModel.Building)));
                return (type, selection);
            }
            case BypassCase.CompoundOnDerivedTypeNotInSelection:
            {
                ResourceType type = _resourceGraph.GetResourceType<ListingModel>();
                FieldSelection selection = new();
                selection.GetOrCreateSelectors(type).IncludeAttribute(type.GetAttributeByPropertyName(nameof(ListingModel.Title)));
                return (type, selection);
            }
            default:
            {
                ResourceType type = _resourceGraph.GetResourceType<PlainModel>();
                FieldSelection selection = new();
                selection.GetOrCreateSelectors(type).IncludeAttribute(type.GetAttributeByPropertyName(nameof(PlainModel.Name)));
                return (type, selection);
            }
        }
    }

    private static FieldSelection CreateParentSelection(ResourceType parentType, ResourceType childType, string relationshipPropertyName)
    {
        RelationshipAttribute relationship = parentType.GetRelationshipByPropertyName(relationshipPropertyName);

        var childSelection = new FieldSelection();
        FieldSelectors childSelectors = childSelection.GetOrCreateSelectors(childType);
        AttrAttribute detailsAttribute = childType.GetAttributeByPropertyName(nameof(ChildModel.Details));
        childSelectors.IncludeAttribute(detailsAttribute);
        childSelectors.GetOrCreateChildSelectors(detailsAttribute).IncludeAttribute(detailsAttribute.CompoundType!.GetAttributeByPropertyName(nameof(ChildDetails.Note)));

        var parentSelection = new FieldSelection();
        FieldSelectors parentSelectors = parentSelection.GetOrCreateSelectors(parentType);
        parentSelectors[relationship] = new QueryLayer(childType)
        {
            Selection = childSelection
        };

        return parentSelection;
    }

    private (bool SelectApplied, bool SourceReturnedUnchanged) ApplySelection(ResourceType resourceType, FieldSelection selection)
    {
        var selectClauseBuilder = new RecordingSelectClauseBuilder();

        var queryableBuilder = new TestableQueryableBuilder(new IncludeClauseBuilder(), new WhereClauseBuilder(), new OrderClauseBuilder(),
            new SkipTakeClauseBuilder(), selectClauseBuilder);

        Type clrType = resourceType.ClrType;
        IQueryable source = Array.CreateInstance(clrType, 0).AsQueryable();
        var context = QueryableBuilderContext.CreateRoot(source, typeof(Queryable), _entityModel, null);

        Expression result = queryableBuilder.CallApplySelection(source.Expression, selection, resourceType, context);

        return (selectClauseBuilder.WasCalled, ReferenceEquals(result, source.Expression));
    }

    private string BuildWhere(string filterText)
    {
        FilterExpression filter = _filterParser.Parse(filterText, _contactType);

        var whereClauseBuilder = new WhereClauseBuilder();
        QueryClauseBuilderContext context = CreateContext(out LambdaScope lambdaScope);

        using (lambdaScope)
        {
            return whereClauseBuilder.ApplyWhere(filter, context).ToString();
        }
    }

    private string BuildOrderBy(SortExpression sort)
    {
        var orderClauseBuilder = new OrderClauseBuilder();
        QueryClauseBuilderContext context = CreateContext(out LambdaScope lambdaScope);

        using (lambdaScope)
        {
            return orderClauseBuilder.ApplyOrderBy(sort, context).ToString();
        }
    }

    private string BuildSelect(FieldSelection selection)
    {
        var selectClauseBuilder = new SelectClauseBuilder(_resourceFactory);
        QueryClauseBuilderContext context = CreateContext(out LambdaScope lambdaScope);

        using (lambdaScope)
        {
            return selectClauseBuilder.ApplySelect(selection, context).ToString();
        }
    }

    private QueryClauseBuilderContext CreateContext(out LambdaScope lambdaScope)
    {
        var lambdaScopeFactory = new LambdaScopeFactory();
        lambdaScope = lambdaScopeFactory.CreateScope(typeof(ContactModel));

        IQueryable<ContactModel> source = Array.Empty<ContactModel>().AsQueryable();

        return new QueryClauseBuilderContext(source.Expression, _contactType, typeof(Queryable), _entityModel, lambdaScopeFactory, lambdaScope,
            new DummyQueryableBuilder(), null);
    }

    private FieldSelection CreateSelection(Action<ChildSelectorBuilder> configureContent)
    {
        var selection = new FieldSelection();
        FieldSelectors selectors = selection.GetOrCreateSelectors(_contactType);
        AttrAttribute contentAttribute = _contactType.GetAttributeByPropertyName(nameof(ContactModel.Content));
        selectors.IncludeAttribute(contentAttribute);

        configureContent(new ChildSelectorBuilder(selectors.GetOrCreateChildSelectors(contentAttribute), contentAttribute.CompoundType!));
        return selection;
    }

    public void Dispose()
    {
        _serviceContainer.Dispose();
    }

    public enum BypassCase
    {
        CompoundOnTop,
        CompoundOnChildToOne,
        CompoundOnChildToMany,
        CompoundOnDerivedTypeInSelection,
        CompoundOnDerivedTypeNotInSelection,
        NoCompound
    }

    private sealed class ChildSelectorBuilder(FieldSelectors selectors, CompoundType compoundType)
    {
        public ChildSelectorBuilder Include(string propertyName, Action<ChildSelectorBuilder>? configureChildren = null)
        {
            AttrAttribute attribute = compoundType.GetAttributeByPropertyName(propertyName);
            selectors.IncludeAttribute(attribute);

            configureChildren?.Invoke(new ChildSelectorBuilder(selectors.GetOrCreateChildSelectors(attribute), attribute.CompoundType!));
            return this;
        }
    }

    private sealed class DummyQueryableBuilder : IQueryableBuilder
    {
        public Expression ApplyQuery(QueryLayer layer, QueryableBuilderContext context)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class RecordingSelectClauseBuilder : ISelectClauseBuilder
    {
        public bool WasCalled { get; private set; }

        public Expression ApplySelect(FieldSelection selection, QueryClauseBuilderContext context)
        {
            WasCalled = true;
            return Expression.Constant("select applied");
        }
    }

    private sealed class TestableQueryableBuilder(
        IIncludeClauseBuilder includeClauseBuilder, IWhereClauseBuilder whereClauseBuilder, IOrderClauseBuilder orderClauseBuilder,
        ISkipTakeClauseBuilder skipTakeClauseBuilder, ISelectClauseBuilder selectClauseBuilder)
        : QueryableBuilder(includeClauseBuilder, whereClauseBuilder, orderClauseBuilder, skipTakeClauseBuilder, selectClauseBuilder)
    {
        public Expression CallApplySelection(Expression source, FieldSelection selection, ResourceType resourceType, QueryableBuilderContext context)
        {
            return ApplySelection(source, selection, resourceType, context);
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ContactModel : Identifiable<long>
    {
        [Attr]
        public string? Name { get; set; }

        [Attr(IsCompound = true)]
        public ContactRoot? Content { get; set; }

        [Attr(IsCompound = true)]
        public List<PhoneNumber> PhoneNumbers { get; set; } = [];

        [Attr]
        public List<string> Tags { get; set; } = [];

        [Attr]
        public int[] Scores { get; set; } = [];

        [Attr]
        public List<string> StringList { get; set; } = [];

        [Attr]
        public List<int?> NullableIntList { get; set; } = [];

        [Attr]
        public List<Guid> GuidList { get; set; } = [];

        [Attr]
        public List<Guid?> NullableGuidList { get; set; } = [];

        [Attr]
        public List<TimeSpan> TimeSpanList { get; set; } = [];
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ContactRoot
    {
        [Attr]
        public string? Title { get; set; }

        [Attr]
        public string? Calculated => Title?.ToUpperInvariant();

        [Attr(IsCompound = true)]
        public PersonName? Name { get; set; }

        [Attr]
        public List<string> Tags { get; set; } = [];

        [Attr(IsCompound = true)]
        public List<PhoneNumber> PhoneNumbers { get; set; } = [];
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class PersonName
    {
        [Attr]
        public string? FirstName { get; set; }

        [Attr]
        public string? LastName { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class PhoneNumber
    {
        [Attr]
        public string? Label { get; set; }

        [Attr]
        public string? Number { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class PlainModel : Identifiable<long>
    {
        [Attr]
        public string? Name { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ChildModel : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public ChildDetails? Details { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ChildDetails
    {
        [Attr]
        public string? Note { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ParentOfOneModel : Identifiable<long>
    {
        [Attr]
        public string? Name { get; set; }

        [HasOne]
        public ChildModel? Child { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ParentOfManyModel : Identifiable<long>
    {
        [Attr]
        public string? Name { get; set; }

        [HasMany]
        public ISet<ChildModel> Children { get; set; } = new HashSet<ChildModel>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private class ListingModel : Identifiable<long>
    {
        [Attr]
        public string? Title { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ApartmentModel : ListingModel
    {
        [Attr(IsCompound = true)]
        public ChildDetails? Building { get; set; }
    }
}
