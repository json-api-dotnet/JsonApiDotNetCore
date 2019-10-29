using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Client;
using Xunit;
using UnitTests.TestModels;
using Person = UnitTests.TestModels.Person;

namespace UnitTests.Serialization.Client
{
    public class RequestSerializerTests : SerializerTestsSetup
    {
        private readonly RequestSerializer _serializer;

        public RequestSerializerTests()
        {
            var builder = new ResourceObjectBuilder(_resourceGraph, new ResourceObjectBuilderSettings());
            _serializer = new RequestSerializer(_resourceGraph, builder);
        }

        [Fact]
        public void SerializeSingle_ResourceWithDefaultTargetFields_CanBuild()
        {
            // Arrange
            var entity = new TestResource() { Id = 1, StringField = "value", NullableIntField = 123 };

            // Act
            string serialized = _serializer.Serialize(entity);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""test-resource"",
                  ""id"":""1"",
                  ""attributes"":{
                     ""string-field"":""value"",
                     ""date-time-field"":""0001-01-01T00:00:00"",
                     ""nullable-date-time-field"":null,
                     ""int-field"":0,
                     ""nullable-int-field"":123,
                     ""guid-field"":""00000000-0000-0000-0000-000000000000"",
                     ""complex-field"":null,
                     ""immutable"":null
                  }
               }
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithTargetedSetAttributes_CanBuild()
        {
            // Arrange
            var entity = new TestResource() { Id = 1, StringField = "value", NullableIntField = 123 };
            _serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // Act
            string serialized = _serializer.Serialize(entity);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""test-resource"",
                  ""id"":""1"",
                  ""attributes"":{
                     ""string-field"":""value""
                  }
               }
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_NoIdWithTargetedSetAttributes_CanBuild()
        {
            // Arrange
            var entityNoId = new TestResource() { Id = 0, StringField = "value", NullableIntField = 123 };
            _serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // Act
            string serialized = _serializer.Serialize(entityNoId);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""test-resource"",
                  ""attributes"":{
                     ""string-field"":""value""
                  }
               }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithoutTargetedAttributes_CanBuild()
        {
            // Arrange
            var entity = new TestResource() { Id = 1, StringField = "value", NullableIntField = 123 };
            _serializer.SetAttributesToSerialize<TestResource>(tr => new { });

            // Act
            string serialized = _serializer.Serialize(entity);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""test-resource"",
                  ""id"":""1""
               }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithTargetedRelationships_CanBuild()
        {
            // Arrange
            var entityWithRelationships = new MultipleRelationshipsPrincipalPart
            {
                PopulatedToOne = new OneToOneDependent { Id = 10 },
                PopulatedToManies = new List<OneToManyDependent> { new OneToManyDependent { Id = 20 } }
            };
            _serializer.SetRelationshipsToSerialize<MultipleRelationshipsPrincipalPart>(tr => new { tr.EmptyToOne, tr.EmptyToManies, tr.PopulatedToOne, tr.PopulatedToManies });

            // Act
            string serialized = _serializer.Serialize(entityWithRelationships);
            Console.WriteLine(serialized);
            // Assert
            var expectedFormatted =
            @"{
                ""data"":{
                    ""type"":""multi-principals"",
                    ""attributes"":{
                        ""attribute-member"":null
                    },
                    ""relationships"":{
                        ""empty-to-one"":{
                        ""data"":null
                        },
                        ""empty-to-manies"":{
                        ""data"":[ ]
                        },
                        ""populated-to-one"":{
                        ""data"":{
                            ""type"":""one-to-one-dependents"",
                            ""id"":""10""
                           }
                        },
                        ""populated-to-manies"":{
                        ""data"":[
                            {
                                ""type"":""one-to-many-dependents"",
                                ""id"":""20""
                            }
                          ]
                        }
                    }
                }
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeMany_ResourcesWithTargetedAttributes_CanBuild()
        {
            // Arrange
            var entities = new List<TestResource>
            {
                new TestResource() { Id = 1, StringField = "value1", NullableIntField = 123 },
                new TestResource() { Id = 2, StringField = "value2", NullableIntField = 123 }
            };
            _serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // Act
            string serialized = _serializer.Serialize(entities);

            // Assert
            var expectedFormatted =
            @"{
                ""data"":[
                    {
                        ""type"":""test-resource"",
                        ""id"":""1"",
                        ""attributes"":{
                        ""string-field"":""value1""
                        }
                    },
                    {
                        ""type"":""test-resource"",
                        ""id"":""2"",
                        ""attributes"":{
                        ""string-field"":""value2""
                        }
                    }
                ]
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_Null_CanBuild()
        {
            // Arrange
            _serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // Act
            IIdentifiable obj = null;
            string serialized = _serializer.Serialize(obj);

            // Assert
            var expectedFormatted =
            @"{
                ""data"":null
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeMany_EmptyList_CanBuild()
        {
            // Arrange
            var entities = new List<TestResource> { };
            _serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // Act
            string serialized = _serializer.Serialize(entities);

            // Assert
            var expectedFormatted =
            @"{
                ""data"":[]
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }
    }
}
