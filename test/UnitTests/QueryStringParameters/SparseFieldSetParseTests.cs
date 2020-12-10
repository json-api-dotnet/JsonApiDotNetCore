using System;
using System.Linq;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using Xunit;

namespace UnitTests.QueryStringParameters
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
            var canParse = _reader.CanRead(parameterName);

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
            var isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

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
        [InlineData("fields[owner.articles]", "id", "Resource type 'owner.articles' does not exist.")]
        [InlineData("fields[articles]", "", "Field name expected.")]
        [InlineData("fields[articles]", " ", "Unexpected whitespace.")]
        [InlineData("fields[articles]", "some", "Field 'some' does not exist on resource 'articles'.")]
        [InlineData("fields[articles]", "id,owner.name", "Field 'owner.name' does not exist on resource 'articles'.")]
        [InlineData("fields[articles]", "id(", ", expected.")]
        [InlineData("fields[articles]", "id,", "Field name expected.")]
        [InlineData("fields[articles]", "author.id,", "Field 'author.id' does not exist on resource 'articles'.")]
        public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read(parameterName, parameterValue);

            // Assert
            var exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be(parameterName);
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified fieldset is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("fields[articles]", "caption,url,author", "articles(caption,url,author)")]
        [InlineData("fields[articles]", "author,revisions,tags", "articles(author,revisions,tags)")]
        [InlineData("fields[countries]", "id", "countries(id)")]
        public void Reader_Read_Succeeds(string parameterName, string parameterValue, string valueExpected)
        {
            // Act
            _reader.Read(parameterName, parameterValue);

            var constraints = _reader.GetConstraints();

            // Assert
            var scope = constraints.Select(x => x.Scope).Single();
            scope.Should().BeNull();

            var value = constraints.Select(x => x.Expression).Single();
            value.ToString().Should().Be(valueExpected);
        }
    }
}
