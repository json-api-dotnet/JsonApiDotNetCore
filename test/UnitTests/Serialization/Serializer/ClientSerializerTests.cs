using System.Collections.Generic;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace UnitTests.Serialization.Serializer
{
    public class ClientSerializerTests : SerializerTestsSetup
    {

        public ClientSerializerTests()
        {
            
        }



        [Fact]
        public void Serialize_TestResource_CanSerialize()
        {
            // arrange
            var complexFieldValue = "complex type field";
            var stringFieldValue = "string field";
            var entity = new TestResource()
            {
                Id = 1,
                ComplexField = new ComplexType() { CompoundName = complexFieldValue },
                StringField = stringFieldValue
            };
            var serializer = GetClientSerializer();

            // act
            var document = serializer.Build(entity);

            // assert
            Assert.Equal(8, document.Data.Attributes.Keys.Count);
            var complexType = (ComplexType)document.Data.Attributes["complex-field"];
            Assert.Equal(complexFieldValue, complexType.CompoundName);
            Assert.Equal(stringFieldValue, document.Data.Attributes["string-field"]);
            Assert.Null(document.Data.Relationships);
            Assert.Equal("1", document.Data.Id);
            Assert.Equal("test-resource", document.Data.Type);
        }

        [Fact]
        public void Serialize_TestResourceList_CanSerialize()
        {
            // arrange
            var entities = new List<TestResource>
            {
                new TestResource { Id = 1 },
                new TestResource { Id = 2 },
                new TestResource { Id = 3 }
            };
            var serializer = GetClientSerializer();

            // act
            var documents = serializer.Build(entities);

            // assert
            Assert.Equal(3, documents.Data.Count);
            foreach (var resourceObject in documents.Data)
            {
                Assert.Equal(8, resourceObject.Attributes.Keys.Count);
                Assert.Null(resourceObject.Relationships);
            }
        }


        [Fact]
        public void Serialize_TestResourceListWithSubsetOfAttributes_CanSerialize()
        {
            // arrange
            var entities = new List<TestResource>
            {
                new TestResource { Id = 1 },
                new TestResource { Id = 2 },
                new TestResource { Id = 3 }
            };
            var serializer = GetClientSerializer();
            serializer.AttributesToInclude<TestResource>(tr => new { tr.StringField, tr.NullableDateTimeField });
            serializer.SetResourceForTests<TestResource>();

            // act
            var documents = serializer.Build(entities);

            // assert
            Assert.Equal(3, documents.Data.Count);
            foreach (var resourceObject in documents.Data)
            {
                Assert.Equal(2, resourceObject.Attributes.Keys.Count);
                Assert.Null(resourceObject.Relationships);
            }
        }

        [Fact]
        public void Serialize_ResourceWithNoAttributes_CanSerialize()
        {
            // arrange
            var serializer = GetClientSerializer();
            serializer.AttributesToInclude<TestResource>(tr => new { });
            serializer.SetResourceForTests<TestResource>();

            // act
            var document = serializer.Build(new TestResource { Id = 1 });

            // assert
            Assert.Null(document.Data.Attributes);
            Assert.Null(document.Data.Relationships);
        }

        [Fact]
        public void Serialize_ResourceWithRelationships_CanSerialize()
        {
            // arrange
            var serializer = GetClientSerializer();
            //serializer.AttributesToInclude<MultipleRelationshipsPrincipalPart>(tr => new { });
            var entity = new MultipleRelationshipsPrincipalPart();


            // act
            var document = serializer.Build(entity);

            // assert
            Assert.Equal(1, document.Data.Attributes.Keys.Count);
            Assert.Null(document.Data.Relationships);
        }

    }
}
