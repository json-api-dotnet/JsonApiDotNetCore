using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class SparseFieldSetParseTests : BaseParseTests
{
    private readonly SparseFieldSetQueryStringParameterReader _reader;

    public SparseFieldSetParseTests()
    {
        var scopeParser = new SparseFieldTypeParser(ResourceGraph);
        var sparseFieldSetParser = new SparseFieldSetParser();
        _reader = new SparseFieldSetQueryStringParameterReader(scopeParser, sparseFieldSetParser, Request, ResourceGraph);
    }

    [Theory]
    [InlineData("fields", false)]
    [InlineData("fields[articles]", true)]
    [InlineData("fields[]", true)]
    [InlineData("fieldset", false)]
    [InlineData("fields[", false)]
    [InlineData("fields]", false)]
    public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
    {
        // Act
        bool canParse = _reader.CanRead(parameterName);

        // Assert
        canParse.Should().Be(expectCanParse);
    }

    [Theory]
    [InlineData(JsonApiQueryStringParameters.Fields, false)]
    [InlineData(JsonApiQueryStringParameters.All, false)]
    [InlineData(JsonApiQueryStringParameters.None, true)]
    [InlineData(JsonApiQueryStringParameters.Filter, true)]
    public void Reader_Is_Enabled(JsonApiQueryStringParameters parametersDisabled, bool expectIsEnabled)
    {
        // Act
        bool isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

        // Assert
        isEnabled.Should().Be(expectIsEnabled);
    }

    [Theory]
    [InlineData("fields^", "[ expected.")]
    [InlineData("fields^]", "[ expected.")]
    [InlineData("fields[^", "Resource type expected.")]
    [InlineData("fields[^]", "Resource type expected.")]
    [InlineData("fields[^ ]", "Unexpected whitespace.")]
    [InlineData("fields[^owner]", "Resource type 'owner' does not exist.")]
    [InlineData("fields[^owner.posts]", "Resource type 'owner' does not exist.")]
    public void Reader_Read_ParameterName_Fails(string parameterName, string errorMessage)
    {
        // Arrange
        var parameterNameSource = new MarkedText(parameterName, '^');

        // Act
        Action action = () => _reader.Read(parameterNameSource.Text, " ");

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterNameSource.Text);
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterNameSource}");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be(parameterNameSource.Text);
    }

    [Theory]
    [InlineData("fields[blogPosts]", "^ ", "Unexpected whitespace.")]
    [InlineData("fields[blogPosts]", "^some", "Field 'some' does not exist on resource type 'blogPosts'.")]
    [InlineData("fields[blogPosts]", "id,^owner.name", "Field 'owner' does not exist on resource type 'blogPosts'.")]
    [InlineData("fields[blogPosts]", "id^(", ", expected.")]
    [InlineData("fields[blogPosts]", "id,^", "Field name expected.")]
    [InlineData("fields[blogPosts]", "author.^id,",
        "Field chain on resource type 'blogPosts' failed to match the pattern: a field. End of field chain expected.")]
    public void Reader_Read_ParameterValue_Fails(string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => _reader.Read(parameterName, parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("fields[blogPosts]", "caption,url,author", "blogPosts(author,caption,url)")]
    [InlineData("fields[blogPosts]", "author,comments,labels", "blogPosts(author,comments,labels)")]
    [InlineData("fields[blogs]", "id", "blogs(id)")]
    [InlineData("fields[blogs]", "", "blogs(id)")]
    public void Reader_Read_Succeeds(string parameterName, string parameterValue, string valueExpected)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

        // Assert
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope.Should().BeNull();

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(valueExpected);
    }
}
