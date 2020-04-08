using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public sealed class SparseFieldsServiceTests : QueryParametersUnitTestCollection
    {
        public SparseFieldsService GetService(ResourceContext resourceContext = null)
        {
            return new SparseFieldsService(_resourceGraph, MockCurrentRequest(resourceContext ?? _articleResourceContext));
        }

        [Fact]
        public void CanParse_SparseFieldsService_SucceedOnMatch()
        {
            // Arrange
            var service = GetService();

            // Act
            bool result = service.CanParse("fields[customer]");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_SparseFieldsService_FailOnMismatch()
        {
            // Arrange
            var service = GetService();

            // Act
            bool result = service.CanParse("fieldset");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Parse_ValidSelection_CanParse()
        {
            // Arrange
            const string type = "articles";
            const string attrName = "someField";
            var attribute = new AttrAttribute(attrName);
            var idAttribute = new AttrAttribute("id");

            var query = new KeyValuePair<string, StringValues>("fields", attrName);

            var resourceContext = new ResourceContext
            {
                ResourceName = type,
                Attributes = new List<AttrAttribute> { attribute, idAttribute },
                Relationships = new List<RelationshipAttribute>()
            };
            var service = GetService(resourceContext);

            // Act
            service.Parse(query.Key, query.Value);
            var result = service.Get();

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(idAttribute, result);
            Assert.Contains(attribute, result);
        }

        [Fact]
        public void Parse_InvalidRelationship_ThrowsJsonApiException()
        {
            // Arrange
            const string type = "articles";
            var attrName = "someField";
            var attribute = new AttrAttribute(attrName);
            var idAttribute = new AttrAttribute("id");
            var queryParameterName = "fields[missing]";

            var query = new KeyValuePair<string, StringValues>(queryParameterName, attrName);

            var resourceContext = new ResourceContext
            {
                ResourceName = type,
                Attributes = new List<AttrAttribute> { attribute, idAttribute },
                Relationships = new List<RelationshipAttribute>()
            };
            var service = GetService(resourceContext);

            // Act, assert
            var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(query.Key, query.Value));
            
            Assert.Equal(queryParameterName, exception.QueryParameterName);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
            Assert.Equal("Sparse field navigation path refers to an invalid relationship.", exception.Error.Title);
            Assert.Equal("'missing' in 'fields[missing]' is not a valid relationship of articles.", exception.Error.Detail);
            Assert.Equal(queryParameterName, exception.Error.Source.Parameter);
        }

        [Fact]
        public void Parse_DeeplyNestedSelection_ThrowsJsonApiException()
        {
            // Arrange
            const string type = "articles";
            const string relationship = "author.employer";
            const string attrName = "someField";
            var attribute = new AttrAttribute(attrName);
            var idAttribute = new AttrAttribute("id");
            var queryParameterName = $"fields[{relationship}]";

            var query = new KeyValuePair<string, StringValues>(queryParameterName, attrName);

            var resourceContext = new ResourceContext
            {
                ResourceName = type,
                Attributes = new List<AttrAttribute> { attribute, idAttribute },
                Relationships = new List<RelationshipAttribute>()
            };
            var service = GetService(resourceContext);

            // Act, assert
            var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(query.Key, query.Value));
            
            Assert.Equal(queryParameterName, exception.QueryParameterName);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
            Assert.Equal("Deeply nested sparse field selection is currently not supported.", exception.Error.Title);
            Assert.Equal($"Parameter fields[{relationship}] is currently not supported.", exception.Error.Detail);
            Assert.Equal(queryParameterName, exception.Error.Source.Parameter);
        }

        [Fact]
        public void Parse_InvalidField_ThrowsJsonApiException()
        {
            // Arrange
            const string type = "articles";
            const string attrName = "dne";
            var idAttribute = new AttrAttribute("id");

            var query = new KeyValuePair<string, StringValues>("fields", attrName);

            var resourceContext = new ResourceContext
            {
                ResourceName = type,
                Attributes = new List<AttrAttribute> {idAttribute},
                Relationships = new List<RelationshipAttribute>()
            };

            var service = GetService(resourceContext);

            // Act, assert
            var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(query.Key, query.Value));
            
            Assert.Equal("fields", exception.QueryParameterName);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
            Assert.Equal("The specified field does not exist on the requested resource.", exception.Error.Title);
            Assert.Equal($"The field '{attrName}' does not exist on resource '{type}'.", exception.Error.Detail);
            Assert.Equal("fields", exception.Error.Source.Parameter);
        }

        [Fact]
        public void Parse_LegacyNotation_ThrowsJsonApiException()
        {
            // Arrange
            const string type = "articles";
            const string attrName = "dne";
            var queryParameterName = $"fields[{type}]";

            var query = new KeyValuePair<string, StringValues>(queryParameterName, attrName);

            var resourceContext = new ResourceContext
            {
                ResourceName = type,
                Attributes = new List<AttrAttribute>(),
                Relationships = new List<RelationshipAttribute>()
            };

            var service = GetService(resourceContext);

            // Act, assert
            var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(query.Key, query.Value));
            
            Assert.Equal(queryParameterName, exception.QueryParameterName);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
            Assert.StartsWith("Square bracket notation in 'filter' is now reserved for relationships only", exception.Error.Title);
            Assert.Equal($"Use '?fields=...' instead of '?fields[{type}]=...'.", exception.Error.Detail);
            Assert.Equal(queryParameterName, exception.Error.Source.Parameter);
        }
    }
}
