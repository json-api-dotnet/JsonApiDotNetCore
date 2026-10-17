using System.ComponentModel.Design;
using System.Net;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class CompoundFilterParseTests
{
    private readonly FilterQueryStringParameterReader _reader;

    public CompoundFilterParseTests()
    {
        var options = new JsonApiOptions { EnableLegacyFilterNotation = false };

        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance)
            .Add<CompoundModel, long>()
            .Build();

        var request = new JsonApiRequest
        {
            PrimaryResourceType = resourceGraph.GetResourceType<CompoundModel>(),
            IsCollection = true
        };

        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new FilterParser(resourceFactory);
        _reader = new FilterQueryStringParameterReader(scopeParser, valueParser, request, resourceGraph, options);
    }

    [Theory]
    [InlineData("filter", "equals(details.title,'abc')")]
    [InlineData("filter", "equals(details,null)")]
    [InlineData("filter", "not(equals(details,null))")]
    [InlineData("filter", "equals(details.nested.description,'test')")]
    [InlineData("filter", "has(items,equals(label,'work'))")]
    [InlineData("filter", "has(details.items,equals(label,'work'))")]
    [InlineData("filter", "greaterThan(count(items),'3')")]
    [InlineData("filter", "greaterThan(count(details.items),'1')")]
    [InlineData("filter", "greaterThan(count(tags),'0')")]
    [InlineData("filter", "greaterThan(count(details.numbers),'2')")]
    [InlineData("filter", "has(tags)")]
    [InlineData("filter", "has(tags,equals(_,'work'))")]
    [InlineData("filter", "has(tags,startsWith(_,'w'))")]
    [InlineData("filter", "has(tags,contains(_,'o'))")]
    [InlineData("filter", "has(tags,endsWith(_,'k'))")]
    [InlineData("filter", "has(tags,any(_,'work'))")]
    [InlineData("filter", "has(tags,not(equals(_,'spam')))")]
    [InlineData("filter", "has(tags,equals(_,null))")]
    [InlineData("filter", "has(details.numbers)")]
    [InlineData("filter", "has(details.numbers,equals(_,'42'))")]
    [InlineData("filter", "has(details.numbers,greaterThan(_,'10'))")]
    public void Reader_Read_Compound_Filter_Succeeds(string parameterName, string parameterValue)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

        // Assert
        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(parameterValue);
    }

    [Theory]
    [InlineData("filter", "greaterThan(details^,null)",
        "Field chain on resource type 'compoundModels' failed to match the pattern: zero or more to-one relationships or compound attributes, followed by an attribute. " +
        "To-one relationship or attribute or compound attribute on compound type 'CompoundRoot' expected.")]
    [InlineData("filter", "equals(details.^unknown,'x')", "Field 'unknown' does not exist on compound type 'CompoundRoot'.")]
    [InlineData("filter", "has(details.numbers,equals(_,^null))", "Function, value between quotes or '_' expected.")]
    [InlineData("filter", "has(tags,equals(^unknown,'work'))", "Function or '_' expected.")]
    [InlineData("filter", "has(tags,^has(tags,equals(_,'w')))", "Function 'has' cannot be used on primitive collection elements.")]
    [InlineData("filter", "has(tags,equals(^count(tags),'1'))", "Function 'count' cannot be used on primitive collection elements.")]
    [InlineData("filter", "has(tags,^isType(,some))", "Function 'isType' cannot be used on primitive collection elements.")]
    public void Reader_Read_Compound_Filter_Fails(string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => _reader.Read(parameterName, parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().Which;
        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Fact]
    public void ElementValueExpression_Properties_And_Equality()
    {
        var expr1 = new ElementValueExpression(typeof(string));
        var expr2 = new ElementValueExpression(typeof(string));
        var expr3 = new ElementValueExpression(typeof(int));

        expr1.ElementType.Should().Be<string>();
        expr1.ToString().Should().Be("_");
        expr1.ToFullString().Should().Be("_");
        expr1.Should().Be(expr2);
        expr1.GetHashCode().Should().Be(expr2.GetHashCode());
        expr1.Should().NotBe(expr3);

        Action createWithNull = () => _ = new ElementValueExpression(null!);
        createWithNull.Should().ThrowExactly<ArgumentNullException>();
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
