using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Newtonsoft.Json;
using Xunit;
using UnitTests.TestModels;

namespace UnitTests.Serialization.Server
{
    public sealed class ResponseSerializerTests : SerializerTestsSetup
    {
        [Fact]
        public void SerializeSingle_ResourceWithDefaultTargetFields_CanSerialize()
        {
            // Arrange
            var entity = new TestResource { Id = 1, StringField = "value", NullableIntField = 123 };
            var serializer = GetResponseSerializer<TestResource>();

            // Act
            string serialized = serializer.SerializeSingle(entity);

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
        public void SerializeMany_ResourceWithDefaultTargetFields_CanSerialize()
        {
            // Arrange
            var entity = new TestResource { Id = 1, StringField = "value", NullableIntField = 123 };
            var serializer = GetResponseSerializer<TestResource>();

            // Act
            string serialized = serializer.SerializeMany(new List<TestResource> { entity });

            // Assert
            var expectedFormatted =
            @"{
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
                     ""complexField"":null,
                     ""immutable"":null
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
            var entity = new MultipleRelationshipsPrincipalPart
            {
                Id = 1,
                PopulatedToOne = new OneToOneDependent { Id = 10 },
                PopulatedToManies = new HashSet<OneToManyDependent> { new OneToManyDependent { Id = 20 } }
            };
            var chain = _resourceGraph.GetRelationships<MultipleRelationshipsPrincipalPart>().Select(r => new List<RelationshipAttribute> { r }).ToList();
            var serializer = GetResponseSerializer<MultipleRelationshipsPrincipalPart>(inclusionChains: chain);

            // Act
            string serialized = serializer.SerializeSingle(entity);

            // Assert
            var expectedFormatted =
            @"{
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
            var deeplyIncludedEntity = new OneToManyPrincipal { Id = 30, AttributeMember = "deep" };
            var includedEntity = new OneToManyDependent { Id = 20, Principal = deeplyIncludedEntity };
            var entity = new MultipleRelationshipsPrincipalPart
            {
                Id = 10,
                PopulatedToManies = new HashSet<OneToManyDependent> { includedEntity }
            };

            var chains = _resourceGraph.GetRelationships<MultipleRelationshipsPrincipalPart>()
                                .Select(r =>
                                {
                                    var chain = new List<RelationshipAttribute> { r };
                                    if (r.PublicRelationshipName != "populatedToManies")
                                        return new List<RelationshipAttribute> { r };
                                    chain.AddRange(_resourceGraph.GetRelationships<OneToManyDependent>());
                                    return chain;
                                }).ToList();

            var serializer = GetResponseSerializer<MultipleRelationshipsPrincipalPart>(inclusionChains: chains);

            // Act
            string serialized = serializer.SerializeSingle(entity);

            // Assert
            var expectedFormatted =
            @"{
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
            var entity = new OneToManyPrincipal { Id = 10 };
            var serializer = GetResponseSerializer<OneToManyPrincipal>(topLinks: _dummyTopLevelLinks, relationshipLinks: _dummyRelationshipLinks, resourceLinks: _dummyResourceLinks);

            // Act
            string serialized = serializer.SerializeSingle(entity);

            // Assert
            var expectedFormatted =
            @"{
               ""links"":{
                  ""self"":""http://www.dummy.com/dummy-self-link"",
                  ""next"":""http://www.dummy.com/dummy-next-link"",
                  ""prev"":""http://www.dummy.com/dummy-prev-link"",
                  ""first"":""http://www.dummy.com/dummy-first-link"",
                  ""last"":""http://www.dummy.com/dummy-last-link""
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
            var entity = new OneToManyPrincipal { Id = 10 };
            var serializer = GetResponseSerializer<OneToManyPrincipal>(metaDict: meta);

            // Act
            string serialized = serializer.SerializeSingle(entity);

            // Assert
            var expectedFormatted =
            @"{
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
            var expectedFormatted =
            @"{
                ""meta"":{ ""test"": ""meta"" },
                ""links"":{
                    ""self"":""http://www.dummy.com/dummy-self-link"",
                    ""next"":""http://www.dummy.com/dummy-next-link"",
                    ""prev"":""http://www.dummy.com/dummy-prev-link"",
                    ""first"":""http://www.dummy.com/dummy-first-link"",
                    ""last"":""http://www.dummy.com/dummy-last-link""
                },
                ""data"": null
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingleWithRequestRelationship_NullToOneRelationship_CanSerialize()
        {
            // Arrange
            var entity = new OneToOnePrincipal { Id = 2, Dependent = null };
            var serializer = GetResponseSerializer<OneToOnePrincipal>();
            var requestRelationship = _resourceGraph.GetRelationships((OneToOnePrincipal t) => t.Dependent).First();
            serializer.RequestRelationship = requestRelationship;

            // Act
            string serialized = serializer.SerializeSingle(entity);

            // Assert
            var expectedFormatted = @"{ ""data"": null}";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingleWithRequestRelationship_PopulatedToOneRelationship_CanSerialize()
        {
            // Arrange
            var entity = new OneToOnePrincipal { Id = 2, Dependent = new OneToOneDependent { Id = 1 } };
            var serializer = GetResponseSerializer<OneToOnePrincipal>();
            var requestRelationship = _resourceGraph.GetRelationships((OneToOnePrincipal t) => t.Dependent).First();
            serializer.RequestRelationship = requestRelationship;


            // Act
            string serialized = serializer.SerializeSingle(entity);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""oneToOneDependents"",
                  ""id"":""1""
               }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");

            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingleWithRequestRelationship_EmptyToManyRelationship_CanSerialize()
        {
            // Arrange
            var entity = new OneToManyPrincipal { Id = 2, Dependents = new HashSet<OneToManyDependent>() };
            var serializer = GetResponseSerializer<OneToManyPrincipal>();
            var requestRelationship = _resourceGraph.GetRelationships((OneToManyPrincipal t) => t.Dependents).First();
            serializer.RequestRelationship = requestRelationship;


            // Act
            string serialized = serializer.SerializeSingle(entity);

            // Assert
            var expectedFormatted = @"{ ""data"": [] }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingleWithRequestRelationship_PopulatedToManyRelationship_CanSerialize()
        {
            // Arrange
            var entity = new OneToManyPrincipal { Id = 2, Dependents = new HashSet<OneToManyDependent> { new OneToManyDependent { Id = 1 } } };
            var serializer = GetResponseSerializer<OneToManyPrincipal>();
            var requestRelationship = _resourceGraph.GetRelationships((OneToManyPrincipal t) => t.Dependents).First();
            serializer.RequestRelationship = requestRelationship;


            // Act
            string serialized = serializer.SerializeSingle(entity);

            // Assert
            var expectedFormatted =
            @"{
               ""data"":[{
                  ""type"":""oneToManyDependents"",
                  ""id"":""1""
               }]
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
