using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public class SparseFieldsServiceTests : QueryParametersUnitTestCollection
    {
        public SparseFieldsService GetService(ContextEntity contextEntity = null)
        {
            return new SparseFieldsService(_graph, MockCurrentRequest(contextEntity ?? _articleResourceContext));
        }

        [Fact]
        public void Name_SparseFieldsService_IsCorrect()
        {
            // arrange
            var filterService = GetService();

            // act
            var name = filterService.Name;

            // assert
            Assert.Equal("fields", name);
        }

        [Fact]
        public void Parse_ValidSelection_CanParse()
        {
            // arrange
            const string type = "articles";
            const string attrName = "some-field";
            const string internalAttrName = "SomeField";
            var attribute = new AttrAttribute(attrName) { InternalAttributeName = internalAttrName };
            var idAttribute = new AttrAttribute("id") { InternalAttributeName = "Id" };

            var query = new KeyValuePair<string, StringValues>($"fields[{type}]", new StringValues(attrName));

            var contextEntity = new ContextEntity
            {
                EntityName = type,
                Attributes = new List<AttrAttribute> { attribute, idAttribute },
                Relationships = new List<RelationshipAttribute>()
            };
            var service = GetService(contextEntity);

            // act
            service.Parse(query);
            var result = service.Get();

            // assert
            Assert.NotEmpty(result);
            Assert.Equal(idAttribute, result.First());
            Assert.Equal(attribute, result[1]);
        }

        [Fact]
        public void Parse_InvalidField_ThrowsJsonApiException()
        {
            // arrange
            const string type = "articles";
            const string attrName = "dne";

            var query = new KeyValuePair<string, StringValues>($"fields[{type}]", new StringValues(attrName));

            var contextEntity = new ContextEntity
            {
                EntityName = type,
                Attributes = new List<AttrAttribute>(),
                Relationships = new List<RelationshipAttribute>()
            };

            var service = GetService(contextEntity);

            // act , assert
            var ex = Assert.Throws<JsonApiException>(() => service.Parse(query));
            Assert.Equal(400, ex.GetStatusCode());
        }
    }
}
