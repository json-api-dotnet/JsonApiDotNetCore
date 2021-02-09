using System;
using System.Linq;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.UnitTests.QueryStringParameters
{
    public sealed class IncludeParseTests : BaseParseTests
    {
        private readonly IncludeQueryStringParameterReader _reader;

        public IncludeParseTests()
        {
            _reader = new IncludeQueryStringParameterReader(Request, ResourceGraph, new JsonApiOptions());
        }

        [Theory]
        [InlineData("include", true)]
        [InlineData("include[some]", false)]
        [InlineData("includes", false)]
        public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
        {
            // Act
            var canParse = _reader.CanRead(parameterName);

            // Assert
            canParse.Should().Be(expectCanParse);
        }

        [Theory]
        [InlineData(StandardQueryStringParameters.Include, false)]
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
        [InlineData("includes", "", "Relationship name expected.")]
        [InlineData("includes", " ", "Unexpected whitespace.")]
        [InlineData("includes", ",", "Relationship name expected.")]
        [InlineData("includes", "posts,", "Relationship name expected.")]
        [InlineData("includes", "posts[", ", expected.")]
        [InlineData("includes", "title", "Relationship 'title' does not exist on resource 'blogs'.")]
        [InlineData("includes", "posts.comments.publishTime,", "Relationship 'publishTime' in 'posts.comments.publishTime' does not exist on resource 'comments'.")]
        public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read(parameterName, parameterValue);

            // Assert
            var exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be(parameterName);
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified include is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("includes", "owner", "owner")]
        [InlineData("includes", "posts", "posts")]
        [InlineData("includes", "owner.posts", "owner.posts")]
        [InlineData("includes", "posts.author", "posts.author")]
        [InlineData("includes", "posts.comments", "posts.comments")]
        [InlineData("includes", "posts,posts.comments", "posts.comments")]
        [InlineData("includes", "posts,posts.comments,posts.labels", "posts.comments,posts.labels")]
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
