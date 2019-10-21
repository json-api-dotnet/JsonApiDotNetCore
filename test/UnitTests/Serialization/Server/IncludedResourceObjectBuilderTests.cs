using JsonApiDotNetCore.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Serialization.Server.Builders;
using UnitTests.TestModels;
using Person = UnitTests.TestModels.Person;

namespace UnitTests.Serialization.Server
{
    public class IncludedResourceObjectBuilderTests : SerializerTestsSetup
    {
        [Fact]
        public void BuildIncluded_DeeplyNestedCircularChainOfSingleData_CanBuild()
        {
            // arrange 
            var (article, author, authorFood, reviewer, reviewerFood) = GetAuthorChainInstances();
            var authorChain = GetIncludedRelationshipsChain("author.blogs.reviewer.favorite-food");
            var builder = GetBuilder();

            // act
            builder.IncludeRelationshipChain(authorChain, article);
            var result = builder.Build();

            // assert
            Assert.Equal(6, result.Count);

            var authorResourceObject = result.Single((ro) => ro.Type == "people" && ro.Id == author.StringId);
            var authorFoodRelation = authorResourceObject.Relationships["favorite-food"].SingleData;
            Assert.Equal(author.FavoriteFood.StringId, authorFoodRelation.Id);

            var reviewerResourceObject = result.Single((ro) => ro.Type == "people" && ro.Id == reviewer.StringId);
            var reviewerFoodRelation = reviewerResourceObject.Relationships["favorite-food"].SingleData;
            Assert.Equal(reviewer.FavoriteFood.StringId, reviewerFoodRelation.Id);
        }

        [Fact]
        public void BuildIncluded_DeeplyNestedCircularChainOfManyData_BuildsWithoutDuplicates()
        {
            // arrange
            var (article, author, _, _, _) = GetAuthorChainInstances();
            var secondArticle = _articleFaker.Generate();
            secondArticle.Author = author;
            var builder = GetBuilder();

            // act
            var authorChain = GetIncludedRelationshipsChain("author.blogs.reviewer.favorite-food");
            builder.IncludeRelationshipChain(authorChain, article);
            builder.IncludeRelationshipChain(authorChain, secondArticle);

            // assert
            var result = builder.Build();
            Assert.Equal(6, result.Count);
        }

        [Fact]
        public void BuildIncluded_OverlappingDeeplyNestedCirculairChains_CanBuild()
        {
            // arrange
            var authorChain = GetIncludedRelationshipsChain("author.blogs.reviewer.favorite-food");
            var (article, author, authorFood, reviewer, reviewerFood) = GetAuthorChainInstances();
            var sharedBlog = author.Blogs.First();
            var sharedBlogAuthor = reviewer;
            var (_reviewer, _reviewerSong, _author, _authorSong) = GetReviewerChainInstances(article, sharedBlog, sharedBlogAuthor);
            var reviewerChain = GetIncludedRelationshipsChain("reviewer.blogs.author.favorite-song");
            var builder = GetBuilder();

            // act
            builder.IncludeRelationshipChain(authorChain, article);
            builder.IncludeRelationshipChain(reviewerChain, article);
            var result = builder.Build();

            // assert
            Assert.Equal(10, result.Count);
            var overlappingBlogResourcObject = result.Single((ro) => ro.Type == "blogs" && ro.Id == sharedBlog.StringId);

            Assert.Equal(2, overlappingBlogResourcObject.Relationships.Keys.ToList().Count);
            var nonOverlappingBlogs = result.Where((ro) => ro.Type == "blogs" && ro.Id != sharedBlog.StringId).ToList();

            foreach (var blog in nonOverlappingBlogs)
                Assert.Equal(1, blog.Relationships.Keys.ToList().Count);

            var sharedAuthorResourceObject = result.Single((ro) => ro.Type == "people" && ro.Id == sharedBlogAuthor.StringId);
            var sharedAuthorSongRelation = sharedAuthorResourceObject.Relationships["favorite-song"].SingleData;
            Assert.Equal(_authorSong.StringId, sharedBlogAuthor.FavoriteSong.StringId);
            var sharedAuthorFoodRelation = sharedAuthorResourceObject.Relationships["favorite-food"].SingleData;
            Assert.Equal(reviewerFood.StringId, sharedBlogAuthor.FavoriteFood.StringId);
        }

        private (Person, Song, Person, Song) GetReviewerChainInstances(Article article, Blog sharedBlog, Person sharedBlogAuthor)
        {
            var reviewer = _personFaker.Generate();
            article.Reviewer = reviewer;

            var blogs = _blogFaker.Generate(1).ToList();
            blogs.Add(sharedBlog);
            reviewer.Blogs = blogs;

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

            var blogs = _blogFaker.Generate(2).ToList();
            author.Blogs = blogs;

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
            var articles = _articleFaker.Generate(5).ToList();
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
            Assert.Equal(1, result.Count);
            Assert.Equal(person.Name, result[0].Attributes["name"]);
            Assert.Equal(person.Id.ToString(), result[0].Id);
        }

        private List<RelationshipAttribute> GetIncludedRelationshipsChain(string chain)
        {
            var parsedChain = new List<RelationshipAttribute>();
            var resourceContext = _resourceGraph.GetContextEntity<Article>();
            var splittedPath = chain.Split(QueryConstants.DOT);
            foreach (var requestedRelationship in splittedPath)
            {
                var relationship = resourceContext.Relationships.Single(r => r.PublicRelationshipName == requestedRelationship);
                parsedChain.Add(relationship);
                resourceContext = _resourceGraph.GetContextEntity(relationship.DependentType);
            }
            return parsedChain;
        }

        private IncludedResourceObjectBuilder GetBuilder()
        {
            var fields = GetSerializableFields();
            var links = GetLinkBuilder();
            return new IncludedResourceObjectBuilder(fields, links, _resourceGraph, GetSerializerSettingsProvider());
        }

    }
}
