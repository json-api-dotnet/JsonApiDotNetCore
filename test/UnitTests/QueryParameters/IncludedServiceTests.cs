using System;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Query;
using Xunit;

namespace UnitTests.QueryParameters
{
    public class IncludedServiceTests : QueryParametersUnitTestCollection
    {

        public IncludeService GetService(ContextEntity resourceContext = null)
        {
            return new IncludeService(resourceContext ?? _articleResourceContext , _graph);
        }

        [Fact]
        public void Parse_ShortChain_CanParse()
        {
            // arrange
            const string chain = "author";

            var service = GetService();

            // act
            service.Parse(null, "author");

            // assert
            var chains = service.Get();
            Assert.Equal(1, chains.Count);
            var relationship = chains.First().First();
            Assert.Equal(chain, relationship.PublicRelationshipName);
        }

    }
}
