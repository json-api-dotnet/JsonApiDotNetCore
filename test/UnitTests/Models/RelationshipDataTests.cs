using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json.Linq;

namespace UnitTests.Models
{
    public class RelationshipDataTests
    {
        [Fact]
        public void Setting_ExposedData_To_List_Sets_ManyData()
        {
            // arrange
            var relationshipData = new RelationshipData();
            var relationships = new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "authors", new { } }
                }
            };

            // act 
            relationshipData.ExposedData = relationships;

            // assert
            Assert.NotEmpty(relationshipData.ManyData);
            Assert.True(relationshipData.ManyData[0].ContainsKey("authors"));
            Assert.True(relationshipData.IsHasMany);
        }

        [Fact]
        public void Setting_ExposedData_To_JArray_Sets_ManyData()
        {
            // arrange
            var relationshipData = new RelationshipData();
            var relationshipsJson = @"[
                {
                    ""authors"": {}
                }
            ]";

            var relationships = JArray.Parse(relationshipsJson);

            // act 
            relationshipData.ExposedData = relationships;

            // assert
            Assert.NotEmpty(relationshipData.ManyData);
            Assert.True(relationshipData.ManyData[0].ContainsKey("authors"));
            Assert.True(relationshipData.IsHasMany);
        }

        [Fact]
        public void Setting_ExposedData_To_Dictionary_Sets_SingleData()
        {
            // arrange
            var relationshipData = new RelationshipData();
            var relationship = new Dictionary<string, object> {
                { "authors", new { } }
            };

            // act 
            relationshipData.ExposedData = relationship;

            // assert
            Assert.NotNull(relationshipData.SingleData);
            Assert.True(relationshipData.SingleData.ContainsKey("authors"));
            Assert.False(relationshipData.IsHasMany);
        }

        [Fact]
        public void Setting_ExposedData_To_JObject_Sets_SingleData()
        {
            // arrange
            var relationshipData = new RelationshipData();
            var relationshipJson = @"{
                    ""authors"": {}
                }";

            var relationship = JObject.Parse(relationshipJson);

            // act 
            relationshipData.ExposedData = relationship;

            // assert
            Assert.NotNull(relationshipData.SingleData);
            Assert.True(relationshipData.SingleData.ContainsKey("authors"));
            Assert.False(relationshipData.IsHasMany);
        }
    }
}
