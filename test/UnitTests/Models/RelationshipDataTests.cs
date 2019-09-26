using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UnitTests.Models
{
    public class RelationshipDataTests
    {
        [Fact]
        public void Setting_ExposeData_To_List_Sets_ManyData()
        {
            // arrange
            var relationshipData = new RelationshipData();
            var relationships = new List<ResourceIdentifierObject> {
                new ResourceIdentifierObject {
                    Id = "9",
                    Type = "authors"
                }
            };

            // act 
            relationshipData.Data = relationships;

            // assert
            Assert.NotEmpty(relationshipData.ManyData);
            Assert.Equal("authors", relationshipData.ManyData[0].Type);
            Assert.Equal("9", relationshipData.ManyData[0].Id);
            Assert.True(relationshipData.IsManyData);
        }

        [Fact]
        public void Setting_ExposeData_To_JArray_Sets_ManyData()
        {
            // arrange
            var relationshipData = new RelationshipData();
            var relationshipsJson = @"[
                {
                    ""type"": ""authors"",
                    ""id"": ""9""
                }
            ]";

            var relationships = JArray.Parse(relationshipsJson);

            // act 
            relationshipData.Data = relationships;

            // assert
            Assert.NotEmpty(relationshipData.ManyData);
            Assert.Equal("authors", relationshipData.ManyData[0].Type);
            Assert.Equal("9", relationshipData.ManyData[0].Id);
            Assert.True(relationshipData.IsManyData);
        }

        [Fact]
        public void Setting_ExposeData_To_RIO_Sets_SingleData()
        {
            // arrange
            var relationshipData = new RelationshipData();
            var relationship = new ResourceIdentifierObject {
                Id = "9",
                Type = "authors"
            };

            // act 
            relationshipData.Data = relationship;

            // assert
            Assert.NotNull(relationshipData.SingleData);
            Assert.Equal("authors", relationshipData.SingleData.Type);
            Assert.Equal("9", relationshipData.SingleData.Id);
            Assert.False(relationshipData.IsManyData);
        }

        [Fact]
        public void Setting_ExposeData_To_JObject_Sets_SingleData()
        {
            // arrange
            var relationshipData = new RelationshipData();
            var relationshipJson = @"{
                    ""id"": ""9"",
                    ""type"": ""authors""
                }";

            var relationship = JObject.Parse(relationshipJson);

            // act 
            relationshipData.Data = relationship;

            // assert
            Assert.NotNull(relationshipData.SingleData);
            Assert.Equal("authors", relationshipData.SingleData.Type);
            Assert.Equal("9", relationshipData.SingleData.Id);
            Assert.False(relationshipData.IsManyData);
        }
    }
}
