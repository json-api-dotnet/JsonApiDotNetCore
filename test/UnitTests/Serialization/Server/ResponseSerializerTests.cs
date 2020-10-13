using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Serialization.Server
{
    public sealed class ResponseSerializerTests : SerializerTestsSetup
    {
        [Fact]
        public void SerializeSingle_ResourceWithDefaultTargetFields_CanSerialize()
        {
            // Arrange
            var resource = new TestResource { Id = 1, StringField = "value", NullableIntField = 123 };
            var serializer = GetResponseSerializer<TestResource>();

            // Act
            string serialized = serializer.SerializeSingle(resource);

            // Assert
            var expectedFormatted = @"{
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
                     ""complexField"":null
                  }
               }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");

            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeMany_ResourceWithDefaultTargetFields_CanSerialize()
        {
            // Arrange
            var resource = new TestResource { Id = 1, StringField = "value", NullableIntField = 123 };
            var serializer = GetResponseSerializer<TestResource>();

            // Act
            string serialized = serializer.SerializeMany(new List<TestResource> { resource });

            // Assert
            var expectedFormatted = @"{
               ""data"":[{
                  ""type"":""testResource"",
                  ""id"":""1"",
                  ""attributes"":{
                     ""stringField"":""value"",
                     ""dateTimeField"":""0001-01-01T00:00:00"",
                     ""nullableDateTimeField"":null,
                     ""intField"":0,
                     ""nullableIntField"":123,
                     ""guidField"":""00000000-0000-0000-0000-000000000000"",
                     ""complexField"":null
                  }
               }]
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithIncludedRelationships_CanSerialize()
        {
            // Arrange
            var resource = new MultipleRelationshipsPrincipalPart
            {
                Id = 1,
                PopulatedToOne = new OneToOneDependent { Id = 10 },
                PopulatedToManies = new HashSet<OneToManyDependent> { new OneToManyDependent { Id = 20 } }
            };
            var chain = _resourceGraph.GetRelationships<MultipleRelationshipsPrincipalPart>().Select(r => new List<RelationshipAttribute> { r }).ToList();
            var serializer = GetResponseSerializer<MultipleRelationshipsPrincipalPart>(inclusionChains: chain);

            // Act
            string serialized = serializer.SerializeSingle(resource);

            // Assert
            var expectedFormatted = @"{
               ""data"":{
                  ""type"":""multiPrincipals"",
                  ""id"":""1"",
                  ""attributes"":{ ""attributeMember"":null },
                  ""relationships"":{
                     ""populatedToOne"":{
                        ""data"":{
                           ""type"":""oneToOneDependents"",
                           ""id"":""10""
                        }
                     },
                     ""emptyToOne"": { ""data"":null },
                     ""populatedToManies"":{
                        ""data"":[
                           {
                              ""type"":""oneToManyDependents"",
                              ""id"":""20""
                           }
                        ]
                     },
                     ""emptyToManies"": { ""data"":[ ] },
                     ""multi"":{ ""data"":null }
                  }
               },
               ""included"":[
                  {
                     ""type"":""oneToOneDependents"",
                     ""id"":""10"",
                     ""attributes"":{ ""attributeMember"":null }
                  },
                  {
                   ""type"":""oneToManyDependents"",
                     ""id"":""20"",
                     ""attributes"":{ ""attributeMember"":null }
                  }
               ]
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithDeeplyIncludedRelationships_CanSerialize()
        {
            // Arrange
            var deeplyIncludedResource = new OneToManyPrincipal { Id = 30, AttributeMember = "deep" };
            var includedResource = new OneToManyDependent { Id = 20, Principal = deeplyIncludedResource };
            var resource = new MultipleRelationshipsPrincipalPart
            {
                Id = 10,
                PopulatedToManies = new HashSet<OneToManyDependent> { includedResource }
            };

            var chains = _resourceGraph.GetRelationships<MultipleRelationshipsPrincipalPart>()
                .Select(r =>
                {
                    var chain = new List<RelationshipAttribute> {r};
                    if (r.PublicName != "populatedToManies")
                        return new List<RelationshipAttribute> {r};
                    chain.AddRange(_resourceGraph.GetRelationships<OneToManyDependent>());
                    return chain;
                }).ToList();

            var serializer = GetResponseSerializer<MultipleRelationshipsPrincipalPart>(inclusionChains: chains);

            // Act
            string serialized = serializer.SerializeSingle(resource);

            // Assert
            var expectedFormatted = @"{
               ""data"":{ 
                  ""type"":""multiPrincipals"",
                  ""id"":""10"",
                  ""attributes"":{ 
                     ""attributeMember"":null
                  },
                  ""relationships"":{ 
                     ""populatedToOne"":{ 
                        ""data"":null
                     },
                     ""emptyToOne"":{ 
                        ""data"":null
                     },
                     ""populatedToManies"":{ 
                        ""data"":[ 
                           { 
                              ""type"":""oneToManyDependents"",
                              ""id"":""20""
                           }
                        ]
                     },
                     ""emptyToManies"":{ 
                        ""data"":[]
                     },
                     ""multi"":{ 
                        ""data"":null
                     }
                  }
               },
               ""included"":[
                  { 
                     ""type"":""oneToManyDependents"",
                     ""id"":""20"",
                     ""attributes"":{ 
                        ""attributeMember"":null
                     },
                     ""relationships"":{ 
                        ""principal"":{ 
                           ""data"":{ 
                              ""type"":""oneToManyPrincipals"",
                              ""id"":""30""
                           }
                        }
                     }
                  },
                  { 
                     ""type"":""oneToManyPrincipals"",
                     ""id"":""30"",
                     ""attributes"":{ 
                        ""attributeMember"":""deep""
                     }
                  }
               ]
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_Null_CanSerialize()
        {
            // Arrange
            var serializer = GetResponseSerializer<TestResource>();
            
            // Act
            string serialized = serializer.SerializeSingle(null);

            // Assert
            var expectedFormatted = @"{ ""data"": null }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeList_EmptyList_CanSerialize()
        {
            // Arrange
            var serializer = GetResponseSerializer<TestResource>();
            // Act
            string serialized = serializer.SerializeMany(new List<TestResource>());

            // Assert
            var expectedFormatted = @"{ ""data"": [] }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithLinksEnabled_CanSerialize()
        {
            // Arrange
            var resource = new OneToManyPrincipal { Id = 10 };
            var serializer = GetResponseSerializer<OneToManyPrincipal>(topLinks: _dummyTopLevelLinks, relationshipLinks: _dummyRelationshipLinks, resourceLinks: _dummyResourceLinks);

            // Act
            string serialized = serializer.SerializeSingle(resource);

            // Assert
            var expectedFormatted = @"{
               ""links"":{
                  ""self"":""http://www.dummy.com/dummy-self-link"",
                  ""first"":""http://www.dummy.com/dummy-first-link"",
                  ""last"":""http://www.dummy.com/dummy-last-link"",
                  ""prev"":""http://www.dummy.com/dummy-prev-link"",
                  ""next"":""http://www.dummy.com/dummy-next-link""
               },
               ""data"":{
                  ""type"":""oneToManyPrincipals"",
                  ""id"":""10"",
                  ""attributes"":{
                     ""attributeMember"":null
                  },
                  ""relationships"":{
                     ""dependents"":{
                        ""links"":{
                           ""self"":""http://www.dummy.com/dummy-relationship-self-link"",
                           ""related"":""http://www.dummy.com/dummy-relationship-related-link""
                        }
                     }
                  },
                  ""links"":{
                     ""self"":""http://www.dummy.com/dummy-resource-self-link""
                  }
               }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithMeta_IncludesMetaInResult()
        {
            // Arrange
            var meta = new Dictionary<string, object> { { "test", "meta" } };
            var resource = new OneToManyPrincipal { Id = 10 };
            var serializer = GetResponseSerializer<OneToManyPrincipal>(metaDict: meta);

            // Act
            string serialized = serializer.SerializeSingle(resource);

            // Assert
            var expectedFormatted = @"{
                ""meta"":{ ""test"": ""meta"" },
                ""data"":{
                    ""type"":""oneToManyPrincipals"",
                    ""id"":""10"",
                    ""attributes"":{
                        ""attributeMember"":null
                    }
                }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_NullWithLinksAndMeta_StillShowsLinksAndMeta()
        {
            // Arrange
            var meta = new Dictionary<string, object> { { "test", "meta" } };
            var serializer = GetResponseSerializer<OneToManyPrincipal>(metaDict: meta, topLinks: _dummyTopLevelLinks, relationshipLinks: _dummyRelationshipLinks, resourceLinks: _dummyResourceLinks);
            
            // Act
            string serialized = serializer.SerializeSingle(null);
            
            // Assert
            var expectedFormatted = @"{
                ""meta"":{ ""test"": ""meta"" },
                ""links"":{
                    ""self"":""http://www.dummy.com/dummy-self-link"",
                    ""first"":""http://www.dummy.com/dummy-first-link"",
                    ""last"":""http://www.dummy.com/dummy-last-link"",
                    ""prev"":""http://www.dummy.com/dummy-prev-link"",
                    ""next"":""http://www.dummy.com/dummy-next-link""
                },
                ""data"": null
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeError_Error_CanSerialize()
        {
            // Arrange
            var error = new Error(HttpStatusCode.InsufficientStorage) {Title = "title", Detail = "detail"};
            var errorDocument = new ErrorDocument(error);

            var expectedJson = JsonConvert.SerializeObject(new
            {
                errors = new[]
                {
                    new
                    {
                        id = error.Id,
                        status = "507",
                        title = "title",
                        detail = "detail"
                    }
                }
            });
            var serializer = GetResponseSerializer<OneToManyPrincipal>();

            // Act
            var result = serializer.Serialize(errorDocument);

            // Assert
            Assert.Equal(expectedJson, result);
        }
    }
}
