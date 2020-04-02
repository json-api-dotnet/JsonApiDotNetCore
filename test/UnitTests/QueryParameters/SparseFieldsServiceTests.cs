using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public void CanParse_FilterService_SucceedOnMatch()
        {
            // Arrange
            var filterService = GetService();

            // Act
            bool result = filterService.CanParse("fields[customer]");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_FilterService_FailOnMismatch()
        {
            // Arrange
            var filterService = GetService();

            // Act
            bool result = filterService.CanParse("other");

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

            var query = new KeyValuePair<string, StringValues>("fields", new StringValues(attrName));

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
            Assert.Equal(idAttribute, result.First());
            Assert.Equal(attribute, result[1]);
        }

        [Fact]
        public void Parse_TypeNameAsNavigation_Throws400ErrorWithRelationshipsOnlyMessage()
        {
            // Arrange
            const string type = "articles";
            const string attrName = "someField";
            var attribute = new AttrAttribute(attrName);
            var idAttribute = new AttrAttribute("id");

            var query = new KeyValuePair<string, StringValues>($"fields[{type}]", new StringValues(attrName));

            var resourceContext = new ResourceContext
            {
                ResourceName = type,
                Attributes = new List<AttrAttribute> { attribute, idAttribute },
                Relationships = new List<RelationshipAttribute>()
            };
            var service = GetService(resourceContext);

            // Act, assert
            var ex = Assert.Throws<JsonApiException>(() => service.Parse(query.Key, query.Value));
            Assert.Contains("relationships only", ex.Message);
            Assert.Equal(HttpStatusCode.BadRequest, ex.Error.Status);
        }

        [Fact]
        public void Parse_DeeplyNestedSelection_Throws400ErrorWithDeeplyNestedMessage()
        {
            // Arrange
            const string type = "articles";
            const string relationship = "author.employer";
            const string attrName = "someField";
            var attribute = new AttrAttribute(attrName);
            var idAttribute = new AttrAttribute("id");

            var query = new KeyValuePair<string, StringValues>($"fields[{relationship}]", new StringValues(attrName));

            var resourceContext = new ResourceContext
            {
                ResourceName = type,
                Attributes = new List<AttrAttribute> { attribute, idAttribute },
                Relationships = new List<RelationshipAttribute>()
            };
            var service = GetService(resourceContext);

            // Act, assert
            var ex = Assert.Throws<JsonApiException>(() => service.Parse(query.Key, query.Value));
            Assert.Contains("deeply nested", ex.Message);
            Assert.Equal(HttpStatusCode.BadRequest, ex.Error.Status);
        }

        [Fact]
        public void Parse_InvalidField_ThrowsJsonApiException()
        {
            // Arrange
            const string type = "articles";
            const string attrName = "dne";

            var query = new KeyValuePair<string, StringValues>($"fields[{type}]", new StringValues(attrName));

            var resourceContext = new ResourceContext
            {
                ResourceName = type,
                Attributes = new List<AttrAttribute>(),
                Relationships = new List<RelationshipAttribute>()
            };

            var service = GetService(resourceContext);

            // Act , assert
            var ex = Assert.Throws<JsonApiException>(() => service.Parse(query.Key, query.Value));
            Assert.Equal(HttpStatusCode.BadRequest, ex.Error.Status);
        }
    }
}
