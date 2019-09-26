using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
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
            var serializer = GetServerSerializer<TestResource>();

            // act
            string serialized = serializer.SerializeSingle(entity);

            // assert
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
        public void SerializeMany_ResourceWithDefaultTargetFields_CanBuild()
        {
            // arrange
            var entity = new TestResource() { Id = 1, StringField = "value", NullableIntField = 123 };
            var serializer = GetServerSerializer<TestResource>();

            // act
            string serialized = serializer.SerializeMany(new List<TestResource> { entity });

            // assert
            var expectedFormatted =
            @"{
               ""data"":[{
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
               }]
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithIncludedRelationships_CanSerialize()
        {
            // arrange
            var entity = new MultipleRelationshipsPrincipalPart
            {
                Id = 1,
                PopulatedToOne = new OneToOneDependent { Id = 10 },
                PopulatedToManies = new List<OneToManyDependent> { new OneToManyDependent { Id = 20 } }
            };
            var chain = _fieldExplorer.GetRelationships<MultipleRelationshipsPrincipalPart>().Select(r => new List<RelationshipAttribute> { r }).ToList();
            var serializer = GetServerSerializer<MultipleRelationshipsPrincipalPart>(inclusionChains: chain);

            // act
            string serialized = serializer.SerializeSingle(entity);

            // assert
            var expectedFormatted =
            @"{
               ""data"":{
                  ""type"":""multi-principals"",
                  ""id"":""1"",
                  ""attributes"":{
                     ""attribute-member"":null
                  },
                  ""relationships"":{
                     ""populated-to-one"":{
                        ""data"":{
                           ""type"":""one-to-one-dependents"",
                           ""id"":""10""
                        }
                     },
                     ""empty-to-one"":{
                        ""data"":null
                     },
                     ""populated-to-manies"":{
                        ""data"":[
                           {
                              ""type"":""one-to-many-dependents"",
                              ""id"":""20""
                           }
                        ]
                     },
                     ""empty-to-manies"":{
                        ""data"":[ ]
                     },
                     ""multi"":{
                        ""data"":null
                     }
                  }
               },
               ""included"":[
                  {
                     ""type"":""one-to-one-dependents"",
                     ""id"":""10"",
                     ""attributes"":{
                        ""attribute-member"":null
                     }
                  },
                  {
                   ""type"":""one-to-many-dependents"",
                     ""id"":""20"",
                     ""attributes"":{
                        ""attribute-member"":null
                     }
                  }
               ]
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithDeeplyIncludedRelationships_CanSerialize()
        {
            // arrange
            var deeplyIncludedEntity = new OneToManyPrincipal { Id = 30, AttributeMember = "deep" };
            var includedEntity = new OneToManyDependent { Id = 20, Principal = deeplyIncludedEntity };
            var entity = new MultipleRelationshipsPrincipalPart
            {
                Id = 10,
                PopulatedToManies = new List<OneToManyDependent> { includedEntity }
            };

            var chains = _fieldExplorer.GetRelationships<MultipleRelationshipsPrincipalPart>()
                                .Select(r =>
                                {
                                    var chain = new List<RelationshipAttribute> { r };
                                    if (r.PublicRelationshipName != "populated-to-manies")
                                        return new List<RelationshipAttribute> { r };
                                    chain.AddRange(_fieldExplorer.GetRelationships<OneToManyDependent>());
                                    return chain;
                                }).ToList();

            var serializer = GetServerSerializer<MultipleRelationshipsPrincipalPart>(inclusionChains: chains);

            // act
            string serialized = serializer.SerializeSingle(entity);

            // assert
            var expectedFormatted =
            @"{
               ""data"":{ 
                  ""type"":""multi-principals"",
                  ""id"":""10"",
                  ""attributes"":{ 
                     ""attribute-member"":null
                  },
                  ""relationships"":{ 
                     ""populated-to-one"":{ 
                        ""data"":null
                     },
                     ""empty-to-one"":{ 
                        ""data"":null
                     },
                     ""populated-to-manies"":{ 
                        ""data"":[ 
                           { 
                              ""type"":""one-to-many-dependents"",
                              ""id"":""20""
                           }
                        ]
                     },
                     ""empty-to-manies"":{ 
                        ""data"":[]
                     },
                     ""multi"":{ 
                        ""data"":null
                     }
                  }
               },
               ""included"":[
                  { 
                     ""type"":""one-to-many-dependents"",
                     ""id"":""20"",
                     ""attributes"":{ 
                        ""attribute-member"":null
                     },
                     ""relationships"":{ 
                        ""principal"":{ 
                           ""data"":{ 
                              ""type"":""one-to-many-principals"",
                              ""id"":""30""
                           }
                        }
                     }
                  },
                  { 
                     ""type"":""one-to-many-principals"",
                     ""id"":""30"",
                     ""attributes"":{ 
                        ""attribute-member"":""deep""
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
            // arrange
            var serializer = GetServerSerializer<TestResource>();
            TestResource entity = null;
            // act
            string serialized = serializer.SerializeSingle(entity);

            // assert
            var expectedFormatted =
            @"{ 
               ""data"": null
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeList_EmptyList_CanSerialize()
        {
            // arrange
            var serializer = GetServerSerializer<TestResource>();
            // act
            string serialized = serializer.SerializeMany(new List<TestResource>());

            // assert
            var expectedFormatted =
            @"{ 
               ""data"": []
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithLinksEnabled_CanSerialize()
        {
            // arrange
            var entity = new OneToManyPrincipal { Id = 10 };
            var includeRelationshipsOn = new List<Type> { typeof(OneToManyPrincipal) };

            var serializer = GetServerSerializer<OneToManyPrincipal>(topLinks: _dummyToplevelLinks, relationshipLinks: _dummyRelationshipLinks, resourceLinks: _dummyResourceLinks);
            // act
            string serialized = serializer.SerializeSingle(entity);

            Console.WriteLine(serialized);
            // assert
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
                  ""type"":""one-to-many-principals"",
                  ""id"":""10"",
                  ""attributes"":{
                     ""attribute-member"":null
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
        public void SerializeSingle_ResourceNoLinksNoRelationships_DoesNotSerializeRelationshipMember()
        {
            // arrange
            var entity = new OneToManyPrincipal { Id = 10 };

            var serializer = GetServerSerializer<OneToManyPrincipal>();
            // act
            string serialized = serializer.SerializeSingle(entity);

            Console.WriteLine(serialized);
            // assert
            var expectedFormatted =
            @"{
                ""data"":{
                    ""type"":""one-to-many-principals"",
                    ""id"":""10"",
                    ""attributes"":{
                        ""attribute-member"":null
                    }
                }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeSingle_ResourceWithMeta_IncludesMetaInResult()
        {
            // arrange
            var meta = new Dictionary<string, object> { { "test", "meta" } };
            var entity = new OneToManyPrincipal { Id = 10 };
            var serializer = GetServerSerializer<OneToManyPrincipal>(metaDict: meta);
            // act
            string serialized = serializer.SerializeSingle(entity);

            Console.WriteLine(serialized);
            // assert
            var expectedFormatted =
            @"{
                ""meta"":{ ""test"": ""meta"" },
                ""data"":{
                    ""type"":""one-to-many-principals"",
                    ""id"":""10"",
                    ""attributes"":{
                        ""attribute-member"":null
                    }
                }
            }";

            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            Assert.Equal(expected, serialized);
        }


        [Fact]
        public void SerializeSingle_NullWithLinksAndMeta_StillShowsLinksAndMeta()
        {
            // arrange
            var meta = new Dictionary<string, object> { { "test", "meta" } };
            OneToManyPrincipal entity = null;
            var serializer = GetServerSerializer<OneToManyPrincipal>(metaDict: meta, topLinks: _dummyToplevelLinks, relationshipLinks: _dummyRelationshipLinks, resourceLinks: _dummyResourceLinks);
            // act
            string serialized = serializer.SerializeSingle(entity);

            Console.WriteLine(serialized);
            // assert
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
    }
}
