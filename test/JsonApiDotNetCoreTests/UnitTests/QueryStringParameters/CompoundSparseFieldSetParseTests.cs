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

public sealed class CompoundSparseFieldSetParseTests
{
    private readonly SparseFieldSetQueryStringParameterReader _reader;

    public CompoundSparseFieldSetParseTests()
    {
        var options = new JsonApiOptions();

        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance)
            .Add<CompoundModel, long>()
            .Build();

        var request = new JsonApiRequest
        {
            PrimaryResourceType = resourceGraph.GetResourceType<CompoundModel>(),
            IsCollection = true
        };

        var typeParser = new SparseFieldTypeParser(resourceGraph);
        var valueParser = new SparseFieldSetParser();
        _reader = new SparseFieldSetQueryStringParameterReader(typeParser, valueParser, request, resourceGraph);
    }

    [Theory]
    [InlineData("fields[compoundModels]", "name", "compoundModels(name)")]
    [InlineData("fields[compoundModels]", "name,details.title", "compoundModels(details.title,name)")]
    [InlineData("fields[compoundModels]", "details.nested.description", "compoundModels(details.nested.description)")]
    [InlineData("fields[compoundModels]", "details.items.label", "compoundModels(details.items.label)")]
    [InlineData("fields[compoundModels]", "details.numbers", "compoundModels(details.numbers)")]
    [InlineData("fields[compoundModels]", "tags", "compoundModels(tags)")]
    [InlineData("fields[compoundModels]", "details", "compoundModels(details)")]
    [InlineData("fields[compoundModels]", "items", "compoundModels(items)")]
    [InlineData("fields[compoundModels]", "parent", "compoundModels(parent)")]
    public void Reader_Read_Compound_SparseFieldSet_Succeeds(string parameterName, string parameterValue, string expectedValue)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

        // Assert
        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("fields[compoundModels]", "details.^unknown", "Field 'unknown' does not exist on compound type 'CompoundRoot'.")]
    [InlineData("fields[compoundModels]", "parent.^name",
        "Field chain on resource type 'compoundModels' failed to match the pattern: zero or more compound attributes or compound collections, followed by a field. " +
        "End of field chain expected.")]
    [InlineData("fields[compoundModels]", "tags.^something",
        "Field chain on resource type 'compoundModels' failed to match the pattern: zero or more compound attributes or compound collections, followed by a field. " +
        "End of field chain expected.")]
    public void Reader_Read_Compound_SparseFieldSet_Fails(string parameterName, string parameterValue, string errorMessage)
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
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class CompoundModel : Identifiable<long>
    {
        [Attr]
        public string? Name { get; set; }

        [HasOne]
        public CompoundModel? Parent { get; set; }

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
