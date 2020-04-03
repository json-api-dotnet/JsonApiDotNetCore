using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.QueryParameters
{
    public sealed class IncludeServiceTests : QueryParametersUnitTestCollection
    {
        public IncludeService GetService(ResourceContext resourceContext = null)
        {
            return new IncludeService(_resourceGraph, MockCurrentRequest(resourceContext ?? _articleResourceContext));
        }

        [Fact]
        public void CanParse_FilterService_SucceedOnMatch()
        {
            // Arrange
            var filterService = GetService();

            // Act
            bool result = filterService.CanParse("include");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_FilterService_FailOnMismatch()
        {
            // Arrange
            var filterService = GetService();

            // Act
            bool result = filterService.CanParse("includes");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Parse_MultipleNestedChains_CanParse()
        {
            // Arrange
            const string chain = "author.blogs.reviewer.favoriteFood,reviewer.blogs.author.favoriteSong";
            var query = new KeyValuePair<string, StringValues>("include", chain);
            var service = GetService();

            // Act
            service.Parse(query.Key, query.Value);

            // Assert
            var chains = service.Get();
            Assert.Equal(2, chains.Count);
            var firstChain = chains[0];
            Assert.Equal("author", firstChain.First().PublicRelationshipName);
            Assert.Equal("favoriteFood", firstChain.Last().PublicRelationshipName);
            var secondChain = chains[1];
            Assert.Equal("reviewer", secondChain.First().PublicRelationshipName);
            Assert.Equal("favoriteSong", secondChain.Last().PublicRelationshipName);
        }

        [Fact]
        public void Parse_ChainsOnWrongMainResource_ThrowsJsonApiException()
        {
            // Arrange
            const string chain = "author.blogs.reviewer.favoriteFood,reviewer.blogs.author.favoriteSong";
            var query = new KeyValuePair<string, StringValues>("include", chain);
            var service = GetService(_resourceGraph.GetResourceContext<Food>());

            // Act, assert
            var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(query.Key, query.Value));

            Assert.Equal("include", exception.QueryParameterName);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
            Assert.Equal("The requested relationship to include does not exist.", exception.Error.Title);
            Assert.Equal("The relationship 'author' on 'foods' does not exist.", exception.Error.Detail);
            Assert.Equal("include", exception.Error.Source.Parameter);
        }

        [Fact]
        public void Parse_NotIncludable_ThrowsJsonApiException()
        {
            // Arrange
            const string chain = "cannotInclude";
            var query = new KeyValuePair<string, StringValues>("include", chain);
            var service = GetService();

            // Act, assert
            var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(query.Key, query.Value));

            Assert.Equal("include", exception.QueryParameterName);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
            Assert.Equal("Including the requested relationship is not allowed.", exception.Error.Title);
            Assert.Equal("Including the relationship 'cannotInclude' on 'articles' is not allowed.", exception.Error.Detail);
            Assert.Equal("include", exception.Error.Source.Parameter);
        }

        [Fact]
        public void Parse_NonExistingRelationship_ThrowsJsonApiException()
        {
            // Arrange
            const string chain = "nonsense";
            var query = new KeyValuePair<string, StringValues>("include", chain);
            var service = GetService();

            // Act, assert
            var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(query.Key, query.Value));

            Assert.Equal("include", exception.QueryParameterName);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
            Assert.Equal("The requested relationship to include does not exist.", exception.Error.Title);
            Assert.Equal("The relationship 'nonsense' on 'articles' does not exist.", exception.Error.Detail);
            Assert.Equal("include", exception.Error.Source.Parameter);
        }
    }
}
