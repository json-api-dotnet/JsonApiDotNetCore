using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using Moq;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Serialization.Server
{
    public sealed class IncludedResourceObjectBuilderTests : SerializerTestsSetup
    {
        [Fact]
        public void BuildIncluded_DeeplyNestedCircularChainOfSingleData_CanBuild()
        {
            // Arrange
            var (article, author, _, reviewer, _) = GetAuthorChainInstances();
            var authorChain = GetIncludedRelationshipsChain("author.blogs.reviewer.favoriteFood");
            var builder = GetBuilder();

            // Act
            builder.IncludeRelationshipChain(authorChain, article);
            var result = builder.Build();

            // Assert
            Assert.Equal(6, result.Count);

            var authorResourceObject = result.Single(ro => ro.Type == "people" && ro.Id == author.StringId);
            var authorFoodRelation = authorResourceObject.Relationships["favoriteFood"].SingleData;
            Assert.Equal(author.FavoriteFood.StringId, authorFoodRelation.Id);

            var reviewerResourceObject = result.Single(ro => ro.Type == "people" && ro.Id == reviewer.StringId);
            var reviewerFoodRelation = reviewerResourceObject.Relationships["favoriteFood"].SingleData;
            Assert.Equal(reviewer.FavoriteFood.StringId, reviewerFoodRelation.Id);
        }

        [Fact]
        public void BuildIncluded_DeeplyNestedCircularChainOfManyData_BuildsWithoutDuplicates()
        {
            // Arrange
            var (article, author, _, _, _) = GetAuthorChainInstances();
            var secondArticle = _articleFaker.Generate();
            secondArticle.Author = author;
            var builder = GetBuilder();

            // Act
            var authorChain = GetIncludedRelationshipsChain("author.blogs.reviewer.favoriteFood");
            builder.IncludeRelationshipChain(authorChain, article);
            builder.IncludeRelationshipChain(authorChain, secondArticle);

            // Assert
            var result = builder.Build();
            Assert.Equal(6, result.Count);
        }

        [Fact]
        public void BuildIncluded_OverlappingDeeplyNestedCircularChains_CanBuild()
        {
            // Arrange
            var authorChain = GetIncludedRelationshipsChain("author.blogs.reviewer.favoriteFood");
            var (article, author, _, reviewer, reviewerFood) = GetAuthorChainInstances();
            var sharedBlog = author.Blogs.First();
            var sharedBlogAuthor = reviewer;
            var (_, _, _, authorSong) = GetReviewerChainInstances(article, sharedBlog, sharedBlogAuthor);
            var reviewerChain = GetIncludedRelationshipsChain("reviewer.blogs.author.favoriteSong");
            var builder = GetBuilder();

            // Act
            builder.IncludeRelationshipChain(authorChain, article);
            builder.IncludeRelationshipChain(reviewerChain, article);
            var result = builder.Build();

            // Assert
            Assert.Equal(10, result.Count);
            var overlappingBlogResourceObject = result.Single(ro => ro.Type == "blogs" && ro.Id == sharedBlog.StringId);

            Assert.Equal(2, overlappingBlogResourceObject.Relationships.Keys.Count);
            var nonOverlappingBlogs = result.Where(ro => ro.Type == "blogs" && ro.Id != sharedBlog.StringId).ToList();

            foreach (var blog in nonOverlappingBlogs)
                Assert.Single(blog.Relationships.Keys);

            Assert.Equal(authorSong.StringId, sharedBlogAuthor.FavoriteSong.StringId);
            Assert.Equal(reviewerFood.StringId, sharedBlogAuthor.FavoriteFood.StringId);
        }

        private (Person, Song, Person, Song) GetReviewerChainInstances(Article article, Blog sharedBlog, Person sharedBlogAuthor)
        {
            var reviewer = _personFaker.Generate();
            article.Reviewer = reviewer;

            var blogs = _blogFaker.Generate(1);
            blogs.Add(sharedBlog);
            reviewer.Blogs = blogs.ToHashSet();

            blogs[0].Author = reviewer;
            var author = _personFaker.Generate();
            blogs[1].Author = sharedBlogAuthor;

            var authorSong = _songFaker.Generate();
            author.FavoriteSong = authorSong;
            sharedBlogAuthor.FavoriteSong = authorSong;

            var reviewerSong = _songFaker.Generate();
            reviewer.FavoriteSong = reviewerSong;

            return (reviewer, reviewerSong, author, authorSong);
        }

        private (Article, Person, Food, Person, Food) GetAuthorChainInstances()
        {
            var article = _articleFaker.Generate();
            var author = _personFaker.Generate();
            article.Author = author;

            var blogs = _blogFaker.Generate(2);
            author.Blogs = blogs.ToHashSet();

            blogs[0].Reviewer = author;
            var reviewer = _personFaker.Generate();
            blogs[1].Reviewer = reviewer;

            var authorFood = _foodFaker.Generate();
            author.FavoriteFood = authorFood;
            var reviewerFood = _foodFaker.Generate();
            reviewer.FavoriteFood = reviewerFood;

            return (article, author, authorFood, reviewer, reviewerFood);
        }

        [Fact]
        public void BuildIncluded_DuplicateChildrenMultipleChains_OnceInOutput()
        {
            var person = _personFaker.Generate();
            var articles = _articleFaker.Generate(5);
            articles.ForEach(a => a.Author = person);
            articles.ForEach(a => a.Reviewer = person);
            var builder = GetBuilder();
            var authorChain = GetIncludedRelationshipsChain("author");
            var reviewerChain = GetIncludedRelationshipsChain("reviewer");
            foreach (var article in articles)
            {
                builder.IncludeRelationshipChain(authorChain, article);
                builder.IncludeRelationshipChain(reviewerChain, article);
            }

            var result = builder.Build();
            Assert.Single(result);
            Assert.Equal(person.Name, result[0].Attributes["name"]);
            Assert.Equal(person.Id.ToString(), result[0].Id);
        }

        private List<RelationshipAttribute> GetIncludedRelationshipsChain(string chain)
        {
            var parsedChain = new List<RelationshipAttribute>();
            var resourceContext = _resourceGraph.GetResourceContext<Article>();
            var splitPath = chain.Split('.');
            foreach (var requestedRelationship in splitPath)
            {
                var relationship = resourceContext.Relationships.Single(r => r.PublicName == requestedRelationship);
                parsedChain.Add(relationship);
                resourceContext = _resourceGraph.GetResourceContext(relationship.RightType);
            }
            return parsedChain;
        }

        private IncludedResourceObjectBuilder GetBuilder()
        {
            var fields = GetSerializableFields();
            var links = GetLinkBuilder();

            var accessor = new Mock<IResourceDefinitionAccessor>().Object;
            return new IncludedResourceObjectBuilder(fields, links, _resourceGraph, Enumerable.Empty<IQueryConstraintProvider>(), accessor, GetSerializerSettingsProvider());
        }
    }
}
