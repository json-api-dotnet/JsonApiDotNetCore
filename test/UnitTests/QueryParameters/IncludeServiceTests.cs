using System;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Query;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.QueryParameters
{
    public class IncludeServiceTests : QueryParametersUnitTestCollection
    {

        public IncludeService GetService(ContextEntity resourceContext = null)
        {
            return new IncludeService(_graph, CurrentRequestMockFactory(resourceContext ?? _articleResourceContext));
        }

        [Fact]
        public void Parse_MultipleNestedChains_CanParse()
        {
            // arrange
            const string chain = "author.blogs.reviewer.favorite-food,reviewer.blogs.author.favorite-song";
            var service = GetService();

            // act
            service.Parse(null, chain);

            // assert
            var chains = service.Get();
            Assert.Equal(2, chains.Count);
            var firstChain = chains[0];
            Assert.Equal("author", firstChain.First().PublicRelationshipName);
            Assert.Equal("favorite-food", firstChain.Last().PublicRelationshipName);
            var secondChain = chains[1];
            Assert.Equal("reviewer", secondChain.First().PublicRelationshipName);
            Assert.Equal("favorite-song", secondChain.Last().PublicRelationshipName);
        }

        [Fact]
        public void Parse_ChainsOnWrongMainResource_ThrowsJsonApiException()
        {
            // arrange
            const string chain = "author.blogs.reviewer.favorite-food,reviewer.blogs.author.favorite-song";
            var service = GetService(_graph.GetContextEntity<Food>());

            // act, assert
            var exception = Assert.Throws<JsonApiException>( () => service.Parse(null, chain));
            Assert.Contains("Invalid", exception.Message);
        }

        [Fact]
        public void Parse_NotIncludable_ThrowsJsonApiException()
        {
            // arrange
            const string chain = "cannot-include";
            var service = GetService();

            // act, assert
            var exception = Assert.Throws<JsonApiException>(() => service.Parse(null, chain));
            Assert.Contains("not allowed", exception.Message);
        }

        [Fact]
        public void Parse_NonExistingRelationship_ThrowsJsonApiException()
        {
            // arrange
            const string chain = "nonsense";
            var service = GetService();

            // act, assert
            var exception = Assert.Throws<JsonApiException>(() => service.Parse(null, chain));
            Assert.Contains("Invalid", exception.Message);
        }

        [Fact]
        public void Parse_EmptyChain_ThrowsJsonApiException()
        {
            // arrange
            const string chain = "";
            var service = GetService();

            // act, assert
            var exception = Assert.Throws<JsonApiException>(() => service.Parse(null, chain));
            Assert.Contains("Include parameter must not be empty if provided", exception.Message);
        }
    }
}
