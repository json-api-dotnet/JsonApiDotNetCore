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
        [InlineData("fields", true)]
        [InlineData("fields[articles]", true)]
        [InlineData("fields[articles.revisions]", true)]
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
        [InlineData("fields[", "id", "Field name expected.")]
        [InlineData("fields[id]", "id", "Relationship 'id' does not exist on resource 'blogs'.")]
        [InlineData("fields[articles.id]", "id", "Relationship 'id' in 'articles.id' does not exist on resource 'articles'.")]
        [InlineData("fields", "", "Attribute name expected.")]
        [InlineData("fields", " ", "Unexpected whitespace.")]
        [InlineData("fields", "id,articles", "Attribute 'articles' does not exist on resource 'blogs'.")]
        [InlineData("fields", "id,articles.name", "Attribute 'articles.name' does not exist on resource 'blogs'.")]
        [InlineData("fields[articles]", "id,tags", "Attribute 'tags' does not exist on resource 'articles'.")]
        [InlineData("fields[articles.author.livingAddress]", "street,some", "Attribute 'some' does not exist on resource 'addresses'.")]
        [InlineData("fields", "id(", ", expected.")]
        [InlineData("fields", "id,", "Attribute name expected.")]
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
        [InlineData("fields", "id", null, "id")]
        [InlineData("fields[articles]", "caption,url", "articles", "caption,url")]
        [InlineData("fields[owner.articles]", "caption", "owner.articles", "caption")]
        [InlineData("fields[articles.author]", "firstName,id", "articles.author", "firstName,id")]
        [InlineData("fields[articles.author.livingAddress]", "street,zipCode", "articles.author.livingAddress", "street,zipCode")]
        [InlineData("fields[articles.tags]", "name,id", "articles.tags", "name,id")]
        public void Reader_Read_Succeeds(string parameterName, string parameterValue, string scopeExpected, string valueExpected)
        {
            // Act
            _reader.Read(parameterName, parameterValue);

            var constraints = _reader.GetConstraints();

            // Assert
            var scope = constraints.Select(x => x.Scope).Single();
            scope?.ToString().Should().Be(scopeExpected);

            var value = constraints.Select(x => x.Expression).Single();
            value.ToString().Should().Be(valueExpected);
        }
    }
}
