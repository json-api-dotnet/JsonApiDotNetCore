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

public sealed class CompoundSortParseTests
{
    private readonly SortQueryStringParameterReader _reader;

    public CompoundSortParseTests()
    {
        var options = new JsonApiOptions();

        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<CompoundModel, long>().Build();

        var request = new JsonApiRequest
        {
            PrimaryResourceType = resourceGraph.GetResourceType<CompoundModel>(),
            IsCollection = true
        };

        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new SortParser();
        _reader = new SortQueryStringParameterReader(scopeParser, valueParser, request, resourceGraph);
    }

    [Theory]
    [InlineData("sort", "name")]
    [InlineData("sort", "details.title")]
    [InlineData("sort", "-details.nested.description")]
    [InlineData("sort", "count(items)")]
    [InlineData("sort", "-count(details.numbers)")]
    [InlineData("sort", "count(tags),details.title")]
    public void Reader_Read_Compound_Sort_Succeeds(string parameterName, string parameterValue)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

        // Assert
        var value = (SortExpression)constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(parameterValue);
    }

    [Theory]
    [InlineData("sort", "details^",
        "Field chain on resource type 'compoundModels' failed to match the pattern: zero or more to-one relationships or compound attributes, followed by an attribute. " +
        "To-one relationship or attribute or compound attribute on compound type 'CompoundRoot' expected.")]
    [InlineData("sort", "^tags",
        "Field chain on resource type 'compoundModels' failed to match the pattern: zero or more to-one relationships or compound attributes, followed by an attribute. " +
        "To-one relationship or attribute or compound attribute on resource type 'compoundModels' expected.")]
    [InlineData("sort", "^items",
        "Field chain on resource type 'compoundModels' failed to match the pattern: zero or more to-one relationships or compound attributes, followed by an attribute. " +
        "To-one relationship or attribute or compound attribute on resource type 'compoundModels' expected.")]
    [InlineData("sort", "count(details^)",
        "Field chain on resource type 'compoundModels' failed to match the pattern: zero or more to-one relationships or compound attributes, followed by a to-many relationship or a primitive collection or a compound collection. " +
        "Relationship or compound attribute or primitive collection or compound collection on compound type 'CompoundRoot' expected.")]
    public void Reader_Read_Compound_Sort_Fails(string parameterName, string parameterValue, string errorMessage)
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
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
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
