using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Serialization.Deserializer
{
    public sealed class BaseDocumentParserTests : DeserializerTestsSetup
    {
        private readonly TestDeserializer _deserializer;

        public BaseDocumentParserTests()
        {
            _deserializer = new TestDeserializer(_resourceGraph, new ResourceFactory(new ServiceContainer()));
        }

        [Fact]
        public void DeserializeResourceIdentifiers_SingleData_CanDeserialize()
        {
            // Arrange
            var content = new Document
            {
                Data = new ResourceObject
                {
                    Type = "testResource",
                    Id = "1"
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (TestResource)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void DeserializeResourceIdentifiers_EmptySingleData_CanDeserialize()
        {
            // Arrange
            var content = new Document();
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.Deserialize(body);

            // Arrange
            Assert.Null(result);
        }

        [Fact]
        public void DeserializeResourceIdentifiers_ArrayData_CanDeserialize()
        {
            // Arrange
            var content = new Document
            {
                Data = new List<ResourceObject>
                {
                    new ResourceObject
                    {
                        Type = "testResource",
                        Id = "1"
                    }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (IEnumerable<IIdentifiable>)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal("1", result.First().StringId);
        }

        [Fact]
        public void DeserializeResourceIdentifiers_EmptyArrayData_CanDeserialize()
        {
            var content = new Document { Data = new List<ResourceObject>()};
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (IEnumerable<IIdentifiable>)_deserializer.Deserialize(body);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("stringField", "some string")]
        [InlineData("stringField", null)]
        [InlineData("intField", null, true)]
        [InlineData("intField", 1)]
        [InlineData("intField", "1")]
        [InlineData("nullableIntField", null)]
        [InlineData("nullableIntField", "1")]
        [InlineData("guidField", "bad format", true)]
        [InlineData("guidField", "1a68be43-cc84-4924-a421-7f4d614b7781")]
        [InlineData("dateTimeField", "9/11/2019 11:41:40 AM")]
        [InlineData("dateTimeField", null, true)]
        [InlineData("nullableDateTimeField", null)]
        public void DeserializeAttributes_VariousDataTypes_CanDeserialize(string member, object value, bool expectError = false)
        {
            // Arrange
            var content = new Document
            {
                Data = new ResourceObject
                {
                    Type = "testResource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { member, value }
                    }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act, assert
            if (expectError)
            {
                Assert.ThrowsAny<FormatException>(() => _deserializer.Deserialize(body));
                return;
            }

            // Act
            var resource = (TestResource)_deserializer.Deserialize(body);

            // Assert
            var pi = _resourceGraph.GetResourceContext("testResource").Attributes.Single(attr => attr.PublicName == member).Property;
            var deserializedValue = pi.GetValue(resource);

            if (member == "intField")
            {
                Assert.Equal(1, deserializedValue);
            }
            else if (member == "nullableIntField" && value == null)
            {
                Assert.Null(deserializedValue);
            }
            else if (member == "nullableIntField" && (string)value == "1")
            {
                Assert.Equal(1, deserializedValue);
            }
            else if (member == "guidField")
            {
                Assert.Equal(deserializedValue, Guid.Parse("1a68be43-cc84-4924-a421-7f4d614b7781"));
            }
            else if (member == "dateTimeField")
            {
                Assert.Equal(deserializedValue, DateTime.Parse("9/11/2019 11:41:40 AM"));
            }
            else
            {
                Assert.Equal(value, deserializedValue);
            }
        }

        [Fact]
        public void DeserializeAttributes_ComplexType_CanDeserialize()
        {
            // Arrange
            var content = new Document
            {
                Data = new ResourceObject
                {
                    Type = "testResource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "complexField", new Dictionary<string, object> { {"compoundName", "testName" } } } // this is not right
                    }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (TestResource)_deserializer.Deserialize(body);

            // Assert
            Assert.NotNull(result.ComplexField);
            Assert.Equal("testName", result.ComplexField.CompoundName);
        }

        [Fact]
        public void DeserializeAttributes_ComplexListType_CanDeserialize()
        {
            // Arrange
            var content = new Document
            {
                Data = new ResourceObject
                {
                    Type = "testResource-with-list",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "complexFields", new [] { new Dictionary<string, object> { {"compoundName", "testName" } } } }
                    }
                }
            };
            var body = JsonConvert.SerializeObject(content);


            // Act
            var result = (TestResourceWithList)_deserializer.Deserialize(body);

            // Assert
            Assert.NotNull(result.ComplexFields);
            Assert.NotEmpty(result.ComplexFields);
            Assert.Equal("testName", result.ComplexFields[0].CompoundName);
        }

        [Fact]
        public void DeserializeRelationship_SingleDataForToOneRelationship_CannotDeserialize()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToManyPrincipals", "dependents");
            content.SingleData.Relationships["dependents"] = new RelationshipEntry { Data = new ResourceIdentifierObject { Type = "Dependents", Id = "1"  } };
            var body = JsonConvert.SerializeObject(content);

            // Act, assert
            Assert.Throws<JsonApiSerializationException>(() => _deserializer.Deserialize(body));
        }

        [Fact]
        public void DeserializeRelationship_ManyDataForToManyRelationship_CannotDeserialize()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToOnePrincipals", "dependent");
            content.SingleData.Relationships["dependent"] = new RelationshipEntry { Data = new List<ResourceIdentifierObject> { new ResourceIdentifierObject { Type = "Dependent", Id = "1"  } }};
            var body = JsonConvert.SerializeObject(content);

            // Act, assert
            Assert.Throws<JsonApiSerializationException>(() => _deserializer.Deserialize(body));
        }

        [Fact]
        public void DeserializeRelationships_EmptyOneToOneDependent_NavigationPropertyIsNull()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToOnePrincipals", "dependent");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToOnePrincipal)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Null(result.Dependent);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_PopulatedOneToOneDependent_NavigationPropertyIsPopulated()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToOnePrincipals", "dependent", "oneToOneDependents");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToOnePrincipal)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Equal(10, result.Dependent.Id);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_EmptyOneToOnePrincipal_NavigationIsNull()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToOneDependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToOneDependent)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Null(result.Principal);
        }

        [Fact]
        public void DeserializeRelationships_EmptyRequiredOneToOnePrincipal_NavigationIsNull()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToOneRequiredDependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToOneRequiredDependent) _deserializer.Deserialize(body);

            // assert
            Assert.Equal(1, result.Id);
            Assert.Null(result.Principal);
        }

        [Fact]
        public void DeserializeRelationships_PopulatedOneToOnePrincipal_NavigationIsPopulated()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToOneDependents", "principal", "oneToOnePrincipals");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToOneDependent)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.Principal);
            Assert.Equal(10, result.Principal.Id);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_EmptyOneToManyPrincipal_NavigationIsNull()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToManyDependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToManyDependent)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Null(result.Principal);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_EmptyOneToManyRequiredPrincipal_NavigationIsNull()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToMany-requiredDependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToManyRequiredDependent) _deserializer.Deserialize(body);

            // assert
            Assert.Equal(1, result.Id);
            Assert.Null(result.Principal);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_PopulatedOneToManyPrincipal_NavigationIsPopulated()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToManyDependents", "principal", "oneToManyPrincipals");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToManyDependent)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.Principal);
            Assert.Equal(10, result.Principal.Id);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_EmptyOneToManyDependent_NavigationIsNull()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToManyPrincipals", "dependents", isToManyData: true);
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToManyPrincipal)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Empty(result.Dependents);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_PopulatedOneToManyDependent_NavigationIsPopulated()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToManyPrincipals", "dependents", "oneToManyDependents", isToManyData: true);
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToManyPrincipal)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Single(result.Dependents);
            Assert.Equal(10, result.Dependents.First().Id);
            Assert.Null(result.AttributeMember);
        }
    }
}
