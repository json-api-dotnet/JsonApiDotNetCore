using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.UnitTests.QueryStringParameters
{
    public sealed class SparseFieldSetParseTests : BaseParseTests
    {
        private readonly SparseFieldSetQueryStringParameterReader _reader;

        public SparseFieldSetParseTests()
        {
            _reader = new SparseFieldSetQueryStringParameterReader(Request, ResourceGraph);
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
        [InlineData(StandardQueryStringParameters.Fields, false)]
        [InlineData(StandardQueryStringParameters.All, false)]
        [InlineData(StandardQueryStringParameters.None, true)]
        [InlineData(StandardQueryStringParameters.Filter, true)]
        public void Reader_Is_Enabled(StandardQueryStringParameters parametersDisabled, bool expectIsEnabled)
        {
            // Act
            bool isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

            // Assert
            isEnabled.Should().Be(expectIsEnabled);
        }

        [Theory]
        [InlineData("fields", "", "[ expected.")]
        [InlineData("fields]", "", "[ expected.")]
        [InlineData("fields[", "", "Resource type expected.")]
        [InlineData("fields[]", "", "Resource type expected.")]
        [InlineData("fields[ ]", "", "Unexpected whitespace.")]
        [InlineData("fields[owner]", "", "Resource type 'owner' does not exist.")]
        [InlineData("fields[owner.posts]", "id", "Resource type 'owner.posts' does not exist.")]
        [InlineData("fields[blogPosts]", "", "Field name expected.")]
        [InlineData("fields[blogPosts]", " ", "Unexpected whitespace.")]
        [InlineData("fields[blogPosts]", "some", "Field 'some' does not exist on resource 'blogPosts'.")]
        [InlineData("fields[blogPosts]", "id,owner.name", "Field 'owner.name' does not exist on resource 'blogPosts'.")]
        [InlineData("fields[blogPosts]", "id(", ", expected.")]
        [InlineData("fields[blogPosts]", "id,", "Field name expected.")]
        [InlineData("fields[blogPosts]", "author.id,", "Field 'author.id' does not exist on resource 'blogPosts'.")]
        public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read(parameterName, parameterValue);

            // Assert
            InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be(parameterName);
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified fieldset is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("fields[blogPosts]", "caption,url,author", "blogPosts(caption,url,author)")]
        [InlineData("fields[blogPosts]", "author,comments,labels", "blogPosts(author,comments,labels)")]
        [InlineData("fields[blogs]", "id", "blogs(id)")]
        public void Reader_Read_Succeeds(string parameterName, string parameterValue, string valueExpected)
        {
            // Act
            _reader.Read(parameterName, parameterValue);

            IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

            // Assert
            ResourceFieldChainExpression scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
            scope.Should().BeNull();

            QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
            value.ToString().Should().Be(valueExpected);
        }
    }
}
