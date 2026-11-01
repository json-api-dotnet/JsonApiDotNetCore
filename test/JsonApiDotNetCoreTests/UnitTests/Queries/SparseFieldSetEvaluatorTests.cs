using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Queries;

public sealed class SparseFieldSetEvaluatorTests
{
    private readonly ResourceType _personType;
    private readonly SparseFieldSetParser _parser = new();
    private readonly PersonDefinition _definition;
    private readonly IResourceDefinitionAccessor _accessor;

    public SparseFieldSetEvaluatorTests()
    {
        IResourceGraph resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<Person, long>().Build();
        _personType = resourceGraph.GetResourceType<Person>();

        _definition = new PersonDefinition(resourceGraph);

        ServiceProvider serviceProvider = new ServiceCollection().AddSingleton<IResourceDefinition<Person, long>>(_definition).BuildServiceProvider();
        _accessor = new ResourceDefinitionAccessor(resourceGraph, serviceProvider);
    }

    [Fact]
    public void Query_side_unions_disjoint_constraints()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("name", "age");

        // Act
        SparseFieldSetExpression? fieldSet = evaluator.GetSparseFieldSetForQuery(_personType);

        // Assert
        fieldSet.Should().NotBeNull();
        fieldSet.ToString().Should().Be("age,name");
    }

    [Fact]
    public void Query_side_whole_compound_beats_child()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("address", "address.city");

        // Act
        SparseFieldSetExpression? fieldSet = evaluator.GetSparseFieldSetForQuery(_personType);

        // Assert
        fieldSet.Should().NotBeNull();
        fieldSet.ToString().Should().Be("address");
        fieldSet.Elements.Values.Single().Children.Should().BeEmpty();
    }

    [Fact]
    public void Query_side_whole_compound_beats_child_regardless_of_order()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("address.city", "address");

        // Act
        SparseFieldSetExpression? fieldSet = evaluator.GetSparseFieldSetForQuery(_personType);

        // Assert
        fieldSet.Should().NotBeNull();
        fieldSet.ToString().Should().Be("address");
    }

    [Fact]
    public void Query_side_unions_children_of_compound()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("address.city", "address.street");

        // Act
        SparseFieldSetExpression? fieldSet = evaluator.GetSparseFieldSetForQuery(_personType);

        // Assert
        fieldSet.Should().NotBeNull();
        fieldSet.ToString().Should().Be("address.city,address.street");
        fieldSet.Elements.Should().HaveCount(1);
        fieldSet.Elements.Values.Single().Children.Should().HaveCount(2);
    }

    [Fact]
    public void Query_side_without_constraint_and_hook_returns_null()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator();

        // Act
        SparseFieldSetExpression? fieldSet = evaluator.GetSparseFieldSetForQuery(_personType);

        // Assert
        fieldSet.Should().BeNull();
    }

    [Fact]
    public void Serializer_side_without_constraint_and_hook_returns_viewable_fields()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator();

        // Act
        SparseFieldSetExpression fieldSet = evaluator.GetSparseFieldSetForSerializer(_personType);

        // Assert
        fieldSet.ToString().Should().Be("address,age,id,name");
        fieldSet.ToString().Should().NotContain("secret");
    }

    [Fact]
    public void Serializer_side_intersects_query_string_with_hook_result()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("name", "age");
        _definition.Callback = _ => ParseFieldSet("name,address");

        // Act
        SparseFieldSetExpression fieldSet = evaluator.GetSparseFieldSetForSerializer(_personType);

        // Assert
        fieldSet.ToString().Should().Be("name");
    }

    [Fact]
    public void Serializer_side_intersection_with_whole_compound_in_first_takes_children_from_second()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("address");
        _definition.Callback = _ => ParseFieldSet("address.city");

        // Act
        SparseFieldSetExpression fieldSet = evaluator.GetSparseFieldSetForSerializer(_personType);

        // Assert
        fieldSet.ToString().Should().Be("address.city");
    }

    [Fact]
    public void Serializer_side_intersection_with_whole_compound_in_second_takes_children_from_first()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("address.city");
        _definition.Callback = _ => ParseFieldSet("address");

        // Act
        SparseFieldSetExpression fieldSet = evaluator.GetSparseFieldSetForSerializer(_personType);

        // Assert
        fieldSet.ToString().Should().Be("address.city");
    }

    [Fact]
    public void Serializer_side_intersection_of_overlapping_children_keeps_common_children()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("address.city,address.street");
        _definition.Callback = _ => ParseFieldSet("address.street,address.zipCode");

        // Act
        SparseFieldSetExpression fieldSet = evaluator.GetSparseFieldSetForSerializer(_personType);

        // Assert
        fieldSet.ToString().Should().Be("address.street");
    }

    [Fact]
    public void Serializer_side_intersection_of_disjoint_children_drops_compound()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("address.city,name");
        _definition.Callback = _ => ParseFieldSet("address.street,name");

        // Act
        SparseFieldSetExpression fieldSet = evaluator.GetSparseFieldSetForSerializer(_personType);

        // Assert
        fieldSet.ToString().Should().Be("name");
    }

    [Fact]
    public void Serializer_side_intersection_of_only_disjoint_children_is_empty()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("address.city");
        _definition.Callback = _ => ParseFieldSet("address.street");

        // Act
        SparseFieldSetExpression fieldSet = evaluator.GetSparseFieldSetForSerializer(_personType);

        // Assert
        fieldSet.Should().BeSameAs(SparseFieldSetExpression.Empty);
    }

    [Fact]
    public void Serializer_side_hook_returning_null_ignores_query_string()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("name");
        _definition.Callback = _ => null;

        // Act
        SparseFieldSetExpression fieldSet = evaluator.GetSparseFieldSetForSerializer(_personType);

        // Assert
        fieldSet.ToString().Should().Be("address,age,id,name");
    }

    [Fact]
    public void Results_are_cached_until_reset()
    {
        // Arrange
        SparseFieldSetEvaluator evaluator = CreateEvaluator("name", "age");
        _definition.Callback = _ => ParseFieldSet("name");

        // Act
        SparseFieldSetExpression? queryBeforeReset = evaluator.GetSparseFieldSetForQuery(_personType);
        SparseFieldSetExpression serializerBeforeReset = evaluator.GetSparseFieldSetForSerializer(_personType);

        _definition.Callback = _ => ParseFieldSet("age");

        SparseFieldSetExpression? queryCached = evaluator.GetSparseFieldSetForQuery(_personType);
        SparseFieldSetExpression serializerCached = evaluator.GetSparseFieldSetForSerializer(_personType);
        int invocationsBeforeReset = _definition.InvocationCount;

        evaluator.Reset();

        SparseFieldSetExpression? queryAfterReset = evaluator.GetSparseFieldSetForQuery(_personType);
        SparseFieldSetExpression serializerAfterReset = evaluator.GetSparseFieldSetForSerializer(_personType);

        // Assert
        queryBeforeReset?.ToString().Should().Be("name");
        serializerBeforeReset.ToString().Should().Be("name");

        queryCached.Should().BeSameAs(queryBeforeReset);
        serializerCached.Should().BeSameAs(serializerBeforeReset);
        invocationsBeforeReset.Should().Be(2);

        queryAfterReset?.ToString().Should().Be("age");
        serializerAfterReset.ToString().Should().Be("age");
        _definition.InvocationCount.Should().Be(4);
    }

    private SparseFieldSetEvaluator CreateEvaluator(params string[] fieldSetTexts)
    {
        ResourceTypeExpressionInScope[] constraints = fieldSetTexts.Select(text => new ResourceTypeExpressionInScope(_personType, ParseFieldSet(text))).ToArray();
        var provider = new FakeConstraintProvider(constraints);

        return new SparseFieldSetEvaluator([provider], _accessor);
    }

    private SparseFieldSetExpression ParseFieldSet(string text)
    {
        return _parser.Parse(text, _personType);
    }

    private sealed class FakeConstraintProvider(IReadOnlyCollection<ExpressionInScope> constraints) : IQueryConstraintProvider
    {
        public IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return constraints;
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Person : Identifiable<long>
    {
        [Attr]
        public string? Name { get; set; }

        [Attr]
        public int Age { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
        public string? Secret { get; set; }

        [Attr(IsCompound = true)]
        public PersonAddress? Address { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class PersonAddress
    {
        [Attr]
        public string? Street { get; set; }

        [Attr]
        public string? City { get; set; }

        [Attr]
        public string? ZipCode { get; set; }
    }

    private sealed class PersonDefinition(IResourceGraph resourceGraph)
        : JsonApiResourceDefinition<Person, long>(resourceGraph)
    {
        internal Func<SparseFieldSetExpression?, SparseFieldSetExpression?> Callback { get; set; } = existing => existing;

        internal int InvocationCount { get; private set; }

        public override SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
        {
            InvocationCount++;
            return Callback(existingSparseFieldSet);
        }
    }
}
