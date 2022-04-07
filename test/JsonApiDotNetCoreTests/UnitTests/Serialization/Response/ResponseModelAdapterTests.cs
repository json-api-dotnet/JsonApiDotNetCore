using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using JsonApiDotNetCoreTests.UnitTests.Serialization.Response.Models;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response;

public sealed class ResponseModelAdapterTests
{
    [Fact]
    public void Resources_in_deeply_nested_circular_chain_are_written_in_relationship_declaration_order()
    {
        // Arrange
        var fakers = new ResponseSerializationFakers();

        Article article = fakers.Article.Generate();
        article.Author = fakers.Person.Generate();
        article.Author.Blogs = fakers.Blog.Generate(2).ToHashSet();
        article.Author.Blogs.ElementAt(0).Reviewer = article.Author;
        article.Author.Blogs.ElementAt(1).Reviewer = fakers.Person.Generate();
        article.Author.FavoriteFood = fakers.Food.Generate();
        article.Author.Blogs.ElementAt(1).Reviewer.FavoriteFood = fakers.Food.Generate();

        IJsonApiOptions options = new JsonApiOptions();
        ResponseModelAdapter responseModelAdapter = CreateAdapter(options, article.StringId, "author.blogs.reviewer.favoriteFood");

        // Act
        Document document = responseModelAdapter.Convert(article);

        // Assert
        string text = JsonSerializer.Serialize(document, new JsonSerializerOptions(options.SerializerWriteOptions));

        text.Should().BeJson(@"{
  ""data"": {
    ""type"": ""articles"",
    ""id"": ""1"",
    ""attributes"": {
      ""title"": ""The SAS microchip is down, quantify the 1080p microchip so we can quantify the SAS microchip!""
    },
    ""relationships"": {
      ""author"": {
        ""data"": {
          ""type"": ""people"",
          ""id"": ""2""
        }
      }
    }
  },
  ""included"": [
    {
      ""type"": ""people"",
      ""id"": ""2"",
      ""attributes"": {
        ""name"": ""Ernestine Runte""
      },
      ""relationships"": {
        ""blogs"": {
          ""data"": [
            {
              ""type"": ""blogs"",
              ""id"": ""3""
            },
            {
              ""type"": ""blogs"",
              ""id"": ""4""
            }
          ]
        },
        ""favoriteFood"": {
          ""data"": {
            ""type"": ""foods"",
            ""id"": ""6""
          }
        }
      }
    },
    {
      ""type"": ""blogs"",
      ""id"": ""3"",
      ""attributes"": {
        ""title"": ""The SAS microchip is down, quantify the 1080p microchip so we can quantify the SAS microchip!""
      },
      ""relationships"": {
        ""reviewer"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""2""
          }
        }
      }
    },
    {
      ""type"": ""blogs"",
      ""id"": ""4"",
      ""attributes"": {
        ""title"": ""I'll connect the mobile ADP card, that should card the ADP card!""
      },
      ""relationships"": {
        ""reviewer"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""5""
          }
        }
      }
    },
    {
      ""type"": ""people"",
      ""id"": ""5"",
      ""attributes"": {
        ""name"": ""Doug Shields""
      },
      ""relationships"": {
        ""favoriteFood"": {
          ""data"": {
            ""type"": ""foods"",
            ""id"": ""7""
          }
        }
      }
    },
    {
      ""type"": ""foods"",
      ""id"": ""7"",
      ""attributes"": {
        ""dish"": ""Nostrum totam harum totam voluptatibus.""
      }
    },
    {
      ""type"": ""foods"",
      ""id"": ""6"",
      ""attributes"": {
        ""dish"": ""Illum assumenda iste quia natus et dignissimos reiciendis.""
      }
    }
  ]
}");
    }

    [Fact]
    public void Resources_in_deeply_nested_circular_chains_are_written_in_relationship_declaration_order_without_duplicates()
    {
        // Arrange
        var fakers = new ResponseSerializationFakers();

        Article article1 = fakers.Article.Generate();
        article1.Author = fakers.Person.Generate();
        article1.Author.Blogs = fakers.Blog.Generate(2).ToHashSet();
        article1.Author.Blogs.ElementAt(0).Reviewer = article1.Author;
        article1.Author.Blogs.ElementAt(1).Reviewer = fakers.Person.Generate();
        article1.Author.FavoriteFood = fakers.Food.Generate();
        article1.Author.Blogs.ElementAt(1).Reviewer.FavoriteFood = fakers.Food.Generate();

        Article article2 = fakers.Article.Generate();
        article2.Author = article1.Author;

        IJsonApiOptions options = new JsonApiOptions();
        ResponseModelAdapter responseModelAdapter = CreateAdapter(options, article1.StringId, "author.blogs.reviewer.favoriteFood");

        // Act
        Document document = responseModelAdapter.Convert(new[]
        {
            article1,
            article2
        });

        // Assert
        string text = JsonSerializer.Serialize(document, new JsonSerializerOptions(options.SerializerWriteOptions));

        text.Should().BeJson(@"{
  ""data"": [
    {
      ""type"": ""articles"",
      ""id"": ""1"",
      ""attributes"": {
        ""title"": ""The SAS microchip is down, quantify the 1080p microchip so we can quantify the SAS microchip!""
      },
      ""relationships"": {
        ""author"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""2""
          }
        }
      }
    },
    {
      ""type"": ""articles"",
      ""id"": ""8"",
      ""attributes"": {
        ""title"": ""I'll connect the mobile ADP card, that should card the ADP card!""
      },
      ""relationships"": {
        ""author"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""2""
          }
        }
      }
    }
  ],
  ""included"": [
    {
      ""type"": ""people"",
      ""id"": ""2"",
      ""attributes"": {
        ""name"": ""Ernestine Runte""
      },
      ""relationships"": {
        ""blogs"": {
          ""data"": [
            {
              ""type"": ""blogs"",
              ""id"": ""3""
            },
            {
              ""type"": ""blogs"",
              ""id"": ""4""
            }
          ]
        },
        ""favoriteFood"": {
          ""data"": {
            ""type"": ""foods"",
            ""id"": ""6""
          }
        }
      }
    },
    {
      ""type"": ""blogs"",
      ""id"": ""3"",
      ""attributes"": {
        ""title"": ""The SAS microchip is down, quantify the 1080p microchip so we can quantify the SAS microchip!""
      },
      ""relationships"": {
        ""reviewer"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""2""
          }
        }
      }
    },
    {
      ""type"": ""blogs"",
      ""id"": ""4"",
      ""attributes"": {
        ""title"": ""I'll connect the mobile ADP card, that should card the ADP card!""
      },
      ""relationships"": {
        ""reviewer"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""5""
          }
        }
      }
    },
    {
      ""type"": ""people"",
      ""id"": ""5"",
      ""attributes"": {
        ""name"": ""Doug Shields""
      },
      ""relationships"": {
        ""favoriteFood"": {
          ""data"": {
            ""type"": ""foods"",
            ""id"": ""7""
          }
        }
      }
    },
    {
      ""type"": ""foods"",
      ""id"": ""7"",
      ""attributes"": {
        ""dish"": ""Nostrum totam harum totam voluptatibus.""
      }
    },
    {
      ""type"": ""foods"",
      ""id"": ""6"",
      ""attributes"": {
        ""dish"": ""Illum assumenda iste quia natus et dignissimos reiciendis.""
      }
    }
  ]
}");
    }

    [Fact]
    public void Resources_in_overlapping_deeply_nested_circular_chains_are_written_in_relationship_declaration_order()
    {
        // Arrange
        var fakers = new ResponseSerializationFakers();

        Article article = fakers.Article.Generate();
        article.Author = fakers.Person.Generate();
        article.Author.Blogs = fakers.Blog.Generate(2).ToHashSet();
        article.Author.Blogs.ElementAt(0).Reviewer = article.Author;
        article.Author.Blogs.ElementAt(1).Reviewer = fakers.Person.Generate();
        article.Author.FavoriteFood = fakers.Food.Generate();
        article.Author.Blogs.ElementAt(1).Reviewer.FavoriteFood = fakers.Food.Generate();

        article.Reviewer = fakers.Person.Generate();
        article.Reviewer.Blogs = fakers.Blog.Generate(1).ToHashSet();
        article.Reviewer.Blogs.Add(article.Author.Blogs.ElementAt(0));
        article.Reviewer.Blogs.ElementAt(0).Author = article.Reviewer;

        article.Reviewer.Blogs.ElementAt(1).Author = article.Author.Blogs.ElementAt(1).Reviewer;
        article.Author.Blogs.ElementAt(1).Reviewer.FavoriteSong = fakers.Song.Generate();
        article.Reviewer.FavoriteSong = fakers.Song.Generate();

        IJsonApiOptions options = new JsonApiOptions();

        ResponseModelAdapter responseModelAdapter =
            CreateAdapter(options, article.StringId, "author.blogs.reviewer.favoriteFood,reviewer.blogs.author.favoriteSong");

        // Act
        Document document = responseModelAdapter.Convert(article);

        // Assert
        string text = JsonSerializer.Serialize(document, new JsonSerializerOptions(options.SerializerWriteOptions));

        text.Should().BeJson(@"{
  ""data"": {
    ""type"": ""articles"",
    ""id"": ""1"",
    ""attributes"": {
      ""title"": ""The SAS microchip is down, quantify the 1080p microchip so we can quantify the SAS microchip!""
    },
    ""relationships"": {
      ""reviewer"": {
        ""data"": {
          ""type"": ""people"",
          ""id"": ""8""
        }
      },
      ""author"": {
        ""data"": {
          ""type"": ""people"",
          ""id"": ""2""
        }
      }
    }
  },
  ""included"": [
    {
      ""type"": ""people"",
      ""id"": ""8"",
      ""attributes"": {
        ""name"": ""Nettie Howell""
      },
      ""relationships"": {
        ""blogs"": {
          ""data"": [
            {
              ""type"": ""blogs"",
              ""id"": ""9""
            },
            {
              ""type"": ""blogs"",
              ""id"": ""3""
            }
          ]
        },
        ""favoriteSong"": {
          ""data"": {
            ""type"": ""songs"",
            ""id"": ""11""
          }
        }
      }
    },
    {
      ""type"": ""blogs"",
      ""id"": ""9"",
      ""attributes"": {
        ""title"": ""The RSS bus is down, parse the mobile bus so we can parse the RSS bus!""
      },
      ""relationships"": {
        ""author"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""8""
          }
        }
      }
    },
    {
      ""type"": ""blogs"",
      ""id"": ""3"",
      ""attributes"": {
        ""title"": ""The SAS microchip is down, quantify the 1080p microchip so we can quantify the SAS microchip!""
      },
      ""relationships"": {
        ""reviewer"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""2""
          }
        },
        ""author"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""5""
          }
        }
      }
    },
    {
      ""type"": ""people"",
      ""id"": ""2"",
      ""attributes"": {
        ""name"": ""Ernestine Runte""
      },
      ""relationships"": {
        ""blogs"": {
          ""data"": [
            {
              ""type"": ""blogs"",
              ""id"": ""3""
            },
            {
              ""type"": ""blogs"",
              ""id"": ""4""
            }
          ]
        },
        ""favoriteFood"": {
          ""data"": {
            ""type"": ""foods"",
            ""id"": ""6""
          }
        }
      }
    },
    {
      ""type"": ""blogs"",
      ""id"": ""4"",
      ""attributes"": {
        ""title"": ""I'll connect the mobile ADP card, that should card the ADP card!""
      },
      ""relationships"": {
        ""reviewer"": {
          ""data"": {
            ""type"": ""people"",
            ""id"": ""5""
          }
        }
      }
    },
    {
      ""type"": ""people"",
      ""id"": ""5"",
      ""attributes"": {
        ""name"": ""Doug Shields""
      },
      ""relationships"": {
        ""favoriteFood"": {
          ""data"": {
            ""type"": ""foods"",
            ""id"": ""7""
          }
        },
        ""favoriteSong"": {
          ""data"": {
            ""type"": ""songs"",
            ""id"": ""10""
          }
        }
      }
    },
    {
      ""type"": ""foods"",
      ""id"": ""7"",
      ""attributes"": {
        ""dish"": ""Nostrum totam harum totam voluptatibus.""
      }
    },
    {
      ""type"": ""songs"",
      ""id"": ""10"",
      ""attributes"": {
        ""title"": ""Illum assumenda iste quia natus et dignissimos reiciendis.""
      }
    },
    {
      ""type"": ""foods"",
      ""id"": ""6"",
      ""attributes"": {
        ""dish"": ""Illum assumenda iste quia natus et dignissimos reiciendis.""
      }
    },
    {
      ""type"": ""songs"",
      ""id"": ""11"",
      ""attributes"": {
        ""title"": ""Nostrum totam harum totam voluptatibus.""
      }
    }
  ]
}");
    }

    [Fact]
    public void Duplicate_children_in_multiple_chains_occur_once_in_output()
    {
        // Arrange
        var fakers = new ResponseSerializationFakers();

        Person person = fakers.Person.Generate();
        List<Article> articles = fakers.Article.Generate(5);
        articles.ForEach(article => article.Author = person);
        articles.ForEach(article => article.Reviewer = person);

        IJsonApiOptions options = new JsonApiOptions();
        ResponseModelAdapter responseModelAdapter = CreateAdapter(options, null, "author,reviewer");

        // Act
        Document document = responseModelAdapter.Convert(articles);

        // Assert
        document.Included.ShouldHaveCount(1);

        document.Included[0].Attributes.ShouldContainKey("name").With(value => value.Should().Be(person.Name));
        document.Included[0].Id.Should().Be(person.StringId);
    }

    private ResponseModelAdapter CreateAdapter(IJsonApiOptions options, string? primaryId, string includeChains)
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance)
            .Add<Article, int>()
            .Add<Person, int>()
            .Add<Blog, int>()
            .Add<Food, int>()
            .Add<Song, int>()
            .Build();

        // @formatter:wrap_chained_method_calls restore
        // @formatter:keep_existing_linebreaks restore

        var request = new JsonApiRequest
        {
            Kind = EndpointKind.Primary,
            PrimaryResourceType = resourceGraph.GetResourceType<Article>(),
            PrimaryId = primaryId
        };

        var evaluatedIncludeCache = new EvaluatedIncludeCache();
        var linkBuilder = new FakeLinkBuilder();
        var metaBuilder = new FakeMetaBuilder();
        var resourceDefinitionAccessor = new FakeResourceDefinitionAccessor();
        var sparseFieldSetCache = new SparseFieldSetCache(Array.Empty<IQueryConstraintProvider>(), resourceDefinitionAccessor);
        var requestQueryStringAccessor = new FakeRequestQueryStringAccessor();

        var parser = new IncludeParser();
        IncludeExpression include = parser.Parse(includeChains, request.PrimaryResourceType, null);
        evaluatedIncludeCache.Set(include);

        return new ResponseModelAdapter(request, options, linkBuilder, metaBuilder, resourceDefinitionAccessor, evaluatedIncludeCache, sparseFieldSetCache,
            requestQueryStringAccessor);
    }
}
