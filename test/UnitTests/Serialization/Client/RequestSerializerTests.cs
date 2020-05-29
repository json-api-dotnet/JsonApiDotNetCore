using System.Collections.Generic;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Client;
using Xunit;
using UnitTests.TestModels;

namespace UnitTests.Serialization.Client
{
    public sealed class RequestSerializerTests : SerializerTestsSetup
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
            var resource = new TestResource { Id = 1, StringField = "value", NullableIntField = 123 };

            // Act
            string serialized = _serializer.Serialize(resource);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""testResource"",
                  ""id"":""1"",
                  ""attributes"":{
                     ""stringField"":""value"",
                     ""dateTimeField"":""0001-01-01T00:00:00"",
                     ""nullableDateTimeField"":null,
                     ""intField"":0,
                     ""nullableIntField"":123,
                     ""guidField"":""00000000-0000-0000-0000-000000000000"",
                     ""complexField"":null,
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
            var resource = new TestResource { Id = 1, StringField = "value", NullableIntField = 123 };
            _serializer.AttributesToSerialize = _resourceGraph.GetAttributes<TestResource>(tr => tr.StringField);

            // Act
            string serialized = _serializer.Serialize(resource);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""testResource"",
                  ""id"":""1"",
                  ""attributes"":{
                     ""stringField"":""value""
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
            var resourceNoId = new TestResource { Id = 0, StringField = "value", NullableIntField = 123 };
            _serializer.AttributesToSerialize = _resourceGraph.GetAttributes<TestResource>(tr => tr.StringField);

            // Act
            string serialized = _serializer.Serialize(resourceNoId);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""testResource"",
                  ""attributes"":{
                     ""stringField"":""value""
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
            var resource = new TestResource { Id = 1, StringField = "value", NullableIntField = 123 };
            _serializer.AttributesToSerialize = _resourceGraph.GetAttributes<TestResource>(tr => new { });

            // Act
            string serialized = _serializer.Serialize(resource);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""testResource"",
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
            var resourceWithRelationships = new MultipleRelationshipsPrincipalPart
            {
                PopulatedToOne = new OneToOneDependent { Id = 10 },
                PopulatedToManies = new HashSet<OneToManyDependent> { new OneToManyDependent { Id = 20 } }
            };
            _serializer.RelationshipsToSerialize = _resourceGraph.GetRelationships<MultipleRelationshipsPrincipalPart>(tr => new { tr.EmptyToOne, tr.EmptyToManies, tr.PopulatedToOne, tr.PopulatedToManies });

            // Act
            string serialized = _serializer.Serialize(resourceWithRelationships);
            // Assert
            var expectedFormatted =
            @"{
                ""data"":{
                    ""type"":""multiPrincipals"",
                    ""attributes"":{
                        ""attributeMember"":null
                    },
                    ""relationships"":{
                        ""emptyToOne"":{
                        ""data"":null
                        },
                        ""emptyToManies"":{
                        ""data"":[ ]
                        },
                        ""populatedToOne"":{
                        ""data"":{
                            ""type"":""oneToOneDependents"",
                            ""id"":""10""
                           }
                        },
                        ""populatedToManies"":{
                        ""data"":[
                            {
                                ""type"":""oneToManyDependents"",
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
            var resources = new List<TestResource>
            {
                new TestResource { Id = 1, StringField = "value1", NullableIntField = 123 },
                new TestResource { Id = 2, StringField = "value2", NullableIntField = 123 }
            };
            _serializer.AttributesToSerialize = _resourceGraph.GetAttributes<TestResource>(tr => tr.StringField);

            // Act
            string serialized = _serializer.Serialize(resources);

            // Assert
            var expectedFormatted =
            @"{
                ""data"":[
                    {
                        ""type"":""testResource"",
                        ""id"":""1"",
                        ""attributes"":{
                        ""stringField"":""value1""
                        }
                    },
                    {
                        ""type"":""testResource"",
                        ""id"":""2"",
                        ""attributes"":{
                        ""stringField"":""value2""
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
            _serializer.AttributesToSerialize = _resourceGraph.GetAttributes<TestResource>(tr => tr.StringField);

            // Act
            string serialized = _serializer.Serialize((IIdentifiable) null);

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
            var resources = new List<TestResource>();
            _serializer.AttributesToSerialize = _resourceGraph.GetAttributes<TestResource>(tr => tr.StringField);

            // Act
            string serialized = _serializer.Serialize(resources);

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
