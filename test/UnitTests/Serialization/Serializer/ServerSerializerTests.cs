using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using Xunit;

namespace UnitTests.Serialization.Serializer
{
    public class ServerSerializerTests : SerializerTestsSetup
    {
        [Fact]
        public void SerializeSingle_ResourceWithDefaultTargetFields_CanBuild()
        {
            // arrange
            var entity = new TestResource() { Id = 1, StringField = "value", NullableIntField = 123 };
            ClientSerializer serializer = GetServerSerializer();

            // act
            string serialized = serializer.Serialize(entity);

            // assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""attributes"":{
                     ""string-field"":""value"",
                     ""date-time-field"":""0001-01-01T00:00:00"",
                     ""nullable-date-time-field"":null,
                     ""int-field"":0,
                     ""nullable-int-field"":123,
                     ""guid-field"":""00000000-0000-0000-0000-000000000000"",
                     ""complex-field"":null,
                     ""immutable"":null
                  },
                  ""type"":""test-resource"",
                  ""id"":""1""
               }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");

            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithTargetedSetAttributes_CanBuild()
        {
            // arrange
            var entity = new TestResource() { Id = 1, StringField = "value", NullableIntField = 123 };
            ClientSerializer serializer = GetServerSerializer();
            serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // act
            string serialized = serializer.Serialize(entity);

            // assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""attributes"":{
                     ""string-field"":""value""
                  },
                  ""type"":""test-resource"",
                  ""id"":""1""
               }
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_NoIdWithTargetedSetAttributes_CanBuild()
        {
            // arrange
            var entityNoId = new TestResource() { Id = 0, StringField = "value", NullableIntField = 123 };
            ClientSerializer serializer = GetServerSerializer();
            serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // act
            string serialized = serializer.Serialize(entityNoId);

            // assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""attributes"":{
                     ""string-field"":""value""
                  },
                  ""type"":""test-resource""
               }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithoutTargetedAttributes_CanBuild()
        {
            // arrange
            var entity = new TestResource() { Id = 1, StringField = "value", NullableIntField = 123 };
            ClientSerializer serializer = GetServerSerializer();
            serializer.SetAttributesToSerialize<TestResource>(tr => new { });

            // act
            string serialized = serializer.Serialize(entity);

            // assert
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
            // arrange
            var entityWithRelationships = new MultipleRelationshipsPrincipalPart
            {
                PopulatedToOne = new OneToOneDependent { Id = 10 },
                PopulatedToManies = new List<OneToManyDependent> { new OneToManyDependent { Id = 20 } }
            };
            ClientSerializer serializer = GetServerSerializer();
            serializer.SetRelationshipsToSerialize<MultipleRelationshipsPrincipalPart>(tr => new { tr.EmptyToOne, tr.EmptyToManies, tr.PopulatedToOne, tr.PopulatedToManies });

            // act
            string serialized = serializer.Serialize(entityWithRelationships);
            Console.WriteLine(serialized);
            // assert
            var expectedFormatted =
            @"{
                ""data"":{
                    ""attributes"":{
                        ""attribute-member"":null
                    },
                    ""relationships"":{
                        ""empty-to-one"":{
                        ""data"":null
                        },
                        ""empty-to-manies"":{
                        ""data"":[

                        ]
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
                    },
                    ""type"":""multi-principals""
                }
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeMany_ResourcesWithTargetedAttributes_CanBuild()
        {
            // arrange
            var entities = new List<TestResource>
            {
                new TestResource() { Id = 1, StringField = "value1", NullableIntField = 123 },
                new TestResource() { Id = 2, StringField = "value2", NullableIntField = 123 }
            };
            ClientSerializer serializer = GetServerSerializer();
            serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // act
            string serialized = serializer.Serialize(entities);

            // assert
            var expectedFormatted =
            @"{
                ""data"":[
                    {
                        ""attributes"":{
                        ""string-field"":""value1""
                        },
                        ""type"":""test-resource"",
                        ""id"":""1""
                    },
                    {
                        ""attributes"":{
                        ""string-field"":""value2""
                        },
                        ""type"":""test-resource"",
                        ""id"":""2""
                    }
                ]
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_Null_CanBuild()
        {
            // arrange
            ClientSerializer serializer = GetServerSerializer();
            serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // act
            IIdentifiable obj = null; ;
            string serialized = serializer.Serialize(obj);

            // assert
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
            // arrange
            var entities = new List<TestResource> { };
            ClientSerializer serializer = GetServerSerializer();
            serializer.SetAttributesToSerialize<TestResource>(tr => tr.StringField);

            // act
            string serialized = serializer.Serialize(entities);

            // assert
            var expectedFormatted =
            @"{
                ""data"":[]
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        private ServerSerializer GetServerSerializer()
        {
            return new ServerSerializer(_fieldExplorer, _resourceGraph, _defaultSettings);
        }
    }
}
