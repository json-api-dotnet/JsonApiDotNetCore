using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Contracts;

using Newtonsoft.Json;
using Xunit;

namespace UnitTests.Deserialization
{
    public class DasherizedResolverTests
    {
        [Fact]
        public void Resolver_Dasherizes_Property_Names()
        {
            // arrange
            var obj = new
            {
                myProp = "val"
            };

            // act
            var result = JsonConvert.SerializeObject(obj,
                Formatting.None,
                new JsonSerializerSettings { ContractResolver = new DasherizedResolver() }
            );

            // assert
            Assert.Equal("{\"my-prop\":\"val\"}", result);
        }
    }
}
