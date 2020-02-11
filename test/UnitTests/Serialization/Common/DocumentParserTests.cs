using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;
using Xunit;
using UnitTests.TestModels;
using Person = UnitTests.TestModels.Person;

namespace UnitTests.Serialization.Deserializer
{
    public class BaseDocumentParserTests : DeserializerTestsSetup
    {
        private readonly TestDocumentParser _deserializer;

        public BaseDocumentParserTests()
        {
            _deserializer = new TestDocumentParser(_resourceGraph);
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
                    Id = "1",
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
            var content = new Document { };
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
                        Id = "1",
                    }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (List<IIdentifiable>)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal("1", result.First().StringId);
        }

        [Fact]
        public void DeserializeResourceIdentifiers_EmptyArrayData_CanDeserialize()
        {
            var content = new Document { Data = new List<ResourceObject> { } };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (IList)_deserializer.Deserialize(body);

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
            var entity = (TestResource)_deserializer.Deserialize(body);

            // Assert
            var pi = _resourceGraph.GetResourceContext("testResource").Attributes.Single(attr => attr.PublicAttributeName == member).PropertyInfo;
            var deserializedValue = pi.GetValue(entity);

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
        public void DeserializeRelationships_EmptyOneToOnePrincipal_NavigationPropertyAndForeignKeyAreNull()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToOneDependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToOneDependent)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Null(result.Principal);
            Assert.Null(result.PrincipalId);
        }

        [Fact]
        public void DeserializeRelationships_EmptyRequiredOneToOnePrincipal_ThrowsFormatException()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToOneRequiredDependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act, assert
            Assert.Throws<FormatException>(() => _deserializer.Deserialize(body));
        }

        [Fact]
        public void DeserializeRelationships_PopulatedOneToOnePrincipal_NavigationPropertyAndForeignKeyArePopulated()
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
            Assert.Equal(10, result.PrincipalId);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_EmptyOneToManyPrincipal_NavigationAndForeignKeyAreNull()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToManyDependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToManyDependent)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Null(result.Principal);
            Assert.Null(result.PrincipalId);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_EmptyOneToManyRequiredPrincipal_ThrowsFormatException()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToMany-requiredDependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act, assert
            Assert.Throws<FormatException>(() => _deserializer.Deserialize(body));
        }

        [Fact]
        public void DeserializeRelationships_PopulatedOneToManyPrincipal_NavigationAndForeignKeyArePopulated()
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
            Assert.Equal(10, result.PrincipalId);
            Assert.Null(result.AttributeMember);
        }

        [Fact]
        public void DeserializeRelationships_EmptyOneToManyDependent_NavigationIsNull()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("oneToManyPrincipals", "dependents");
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = (OneToManyPrincipal)_deserializer.Deserialize(body);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Null(result.Dependents);
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
