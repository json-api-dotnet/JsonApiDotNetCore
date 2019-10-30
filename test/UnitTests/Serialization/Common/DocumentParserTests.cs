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
            // arange
            var content = new Document
            {
                Data = new ResourceObject
                {
                    Type = "test-resource",
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
            // arange
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
            // arange
            var content = new Document
            {
                Data = new List<ResourceObject>
                {
                    new ResourceObject
                    {
                        Type = "test-resource",
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
        [InlineData("string-field", "some string")]
        [InlineData("string-field", null)]
        [InlineData("int-field", null, true)]
        [InlineData("int-field", 1)]
        [InlineData("int-field", "1")]
        [InlineData("nullable-int-field", null)]
        [InlineData("nullable-int-field", "1")]
        [InlineData("guid-field", "bad format", true)]
        [InlineData("guid-field", "1a68be43-cc84-4924-a421-7f4d614b7781")]
        [InlineData("date-time-field", "9/11/2019 11:41:40 AM")]
        [InlineData("date-time-field", null, true)]
        [InlineData("nullable-date-time-field", null)]
        public void DeserializeAttributes_VariousDataTypes_CanDeserialize(string member, object value, bool expectError = false)
        {
            // Arrange
            var content = new Document
            {
                Data = new ResourceObject
                {
                    Type = "test-resource",
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
            var pi = _resourceGraph.GetResourceContext("test-resource").Attributes.Single(attr => attr.PublicAttributeName == member).PropertyInfo;
            var deserializedValue = pi.GetValue(entity);

            if (member == "int-field")
            {
                Assert.Equal(1, deserializedValue);
            }
            else if (member == "nullable-int-field" && value == null)
            {
                Assert.Null(deserializedValue);
            }
            else if (member == "nullable-int-field" && (string)value == "1")
            {
                Assert.Equal(1, deserializedValue);
            }
            else if (member == "guid-field")
            {
                Assert.Equal(deserializedValue, Guid.Parse("1a68be43-cc84-4924-a421-7f4d614b7781"));
            }
            else if (member == "date-time-field")
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
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "complex-field", new Dictionary<string, object> { {"compoundName", "testName" } } } // this is not right
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
                    Type = "test-resource-with-list",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "complex-fields", new [] { new Dictionary<string, object> { {"compoundName", "testName" } } } }
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
            var content = CreateDocumentWithRelationships("one-to-one-principals", "dependent");
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
            var content = CreateDocumentWithRelationships("one-to-one-principals", "dependent", "one-to-one-dependents");
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
            var content = CreateDocumentWithRelationships("one-to-one-dependents", "principal");
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
            var content = CreateDocumentWithRelationships("one-to-one-required-dependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act, assert
            Assert.Throws<FormatException>(() => _deserializer.Deserialize(body));
        }

        [Fact]
        public void DeserializeRelationships_PopulatedOneToOnePrincipal_NavigationPropertyAndForeignKeyArePopulated()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("one-to-one-dependents", "principal", "one-to-one-principals");
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
            var content = CreateDocumentWithRelationships("one-to-many-dependents", "principal");
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
            var content = CreateDocumentWithRelationships("one-to-many-required-dependents", "principal");
            var body = JsonConvert.SerializeObject(content);

            // Act, assert
            Assert.Throws<FormatException>(() => _deserializer.Deserialize(body));
        }

        [Fact]
        public void DeserializeRelationships_PopulatedOneToManyPrincipal_NavigationAndForeignKeyArePopulated()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("one-to-many-dependents", "principal", "one-to-many-principals");
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
            var content = CreateDocumentWithRelationships("one-to-many-principals", "dependents");
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
            var content = CreateDocumentWithRelationships("one-to-many-principals", "dependents", "one-to-many-dependents", isToManyData: true);
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
