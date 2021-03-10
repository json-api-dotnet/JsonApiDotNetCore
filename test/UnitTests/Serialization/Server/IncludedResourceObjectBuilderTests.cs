using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
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
            (Article article, Person author, _, Person reviewer, _) = GetAuthorChainInstances();
            IReadOnlyCollection<RelationshipAttribute> authorChain = GetIncludedRelationshipsChain("author.blogs.reviewer.favoriteFood");
            IncludedResourceObjectBuilder builder = GetBuilder();

            // Act
            builder.IncludeRelationshipChain(authorChain, article);
            IList<ResourceObject> result = builder.Build();

            // Assert
            Assert.Equal(6, result.Count);

            ResourceObject authorResourceObject = result.Single(ro => ro.Type == "people" && ro.Id == author.StringId);
            ResourceIdentifierObject authorFoodRelation = authorResourceObject.Relationships["favoriteFood"].SingleData;
            Assert.Equal(author.FavoriteFood.StringId, authorFoodRelation.Id);

            ResourceObject reviewerResourceObject = result.Single(ro => ro.Type == "people" && ro.Id == reviewer.StringId);
            ResourceIdentifierObject reviewerFoodRelation = reviewerResourceObject.Relationships["favoriteFood"].SingleData;
            Assert.Equal(reviewer.FavoriteFood.StringId, reviewerFoodRelation.Id);
        }

        [Fact]
        public void BuildIncluded_DeeplyNestedCircularChainOfManyData_BuildsWithoutDuplicates()
        {
            // Arrange
            (Article article, Person author, _, _, _) = GetAuthorChainInstances();
            Article secondArticle = ArticleFaker.Generate();
            secondArticle.Author = author;
            IncludedResourceObjectBuilder builder = GetBuilder();

            // Act
            IReadOnlyCollection<RelationshipAttribute> authorChain = GetIncludedRelationshipsChain("author.blogs.reviewer.favoriteFood");
            builder.IncludeRelationshipChain(authorChain, article);
            builder.IncludeRelationshipChain(authorChain, secondArticle);

            // Assert
            IList<ResourceObject> result = builder.Build();
            Assert.Equal(6, result.Count);
        }

        [Fact]
        public void BuildIncluded_OverlappingDeeplyNestedCircularChains_CanBuild()
        {
            // Arrange
            IReadOnlyCollection<RelationshipAttribute> authorChain = GetIncludedRelationshipsChain("author.blogs.reviewer.favoriteFood");
            (Article article, Person author, _, Person reviewer, Food reviewerFood) = GetAuthorChainInstances();
            Blog sharedBlog = author.Blogs.First();
            Person sharedBlogAuthor = reviewer;
            Song authorSong = GetReviewerChainInstances(article, sharedBlog, sharedBlogAuthor);
            IReadOnlyCollection<RelationshipAttribute> reviewerChain = GetIncludedRelationshipsChain("reviewer.blogs.author.favoriteSong");
            IncludedResourceObjectBuilder builder = GetBuilder();

            // Act
            builder.IncludeRelationshipChain(authorChain, article);
            builder.IncludeRelationshipChain(reviewerChain, article);
            IList<ResourceObject> result = builder.Build();

            // Assert
            Assert.Equal(10, result.Count);
            ResourceObject overlappingBlogResourceObject = result.Single(ro => ro.Type == "blogs" && ro.Id == sharedBlog.StringId);

            Assert.Equal(2, overlappingBlogResourceObject.Relationships.Keys.Count);
            List<ResourceObject> nonOverlappingBlogs = result.Where(ro => ro.Type == "blogs" && ro.Id != sharedBlog.StringId).ToList();

            foreach (ResourceObject blog in nonOverlappingBlogs)
            {
                Assert.Single(blog.Relationships.Keys);
            }

            Assert.Equal(authorSong.StringId, sharedBlogAuthor.FavoriteSong.StringId);
            Assert.Equal(reviewerFood.StringId, sharedBlogAuthor.FavoriteFood.StringId);
        }

        [Fact]
        public void BuildIncluded_DuplicateChildrenMultipleChains_OnceInOutput()
        {
            Person person = PersonFaker.Generate();
            List<Article> articles = ArticleFaker.Generate(5);
            articles.ForEach(article => article.Author = person);
            articles.ForEach(article => article.Reviewer = person);
            IncludedResourceObjectBuilder builder = GetBuilder();
            IReadOnlyCollection<RelationshipAttribute> authorChain = GetIncludedRelationshipsChain("author");
            IReadOnlyCollection<RelationshipAttribute> reviewerChain = GetIncludedRelationshipsChain("reviewer");

            foreach (Article article in articles)
            {
                builder.IncludeRelationshipChain(authorChain, article);
                builder.IncludeRelationshipChain(reviewerChain, article);
            }

            IList<ResourceObject> result = builder.Build();
            Assert.Single(result);
            Assert.Equal(person.Name, result[0].Attributes["name"]);
            Assert.Equal(person.Id.ToString(), result[0].Id);
        }

        private Song GetReviewerChainInstances(Article article, Blog sharedBlog, Person sharedBlogAuthor)
        {
            Person reviewer = PersonFaker.Generate();
            article.Reviewer = reviewer;

            List<Blog> blogs = BlogFaker.Generate(1);
            blogs.Add(sharedBlog);
            reviewer.Blogs = blogs.ToHashSet();

            blogs[0].Author = reviewer;
            Person author = PersonFaker.Generate();
            blogs[1].Author = sharedBlogAuthor;

            Song authorSong = SongFaker.Generate();
            author.FavoriteSong = authorSong;
            sharedBlogAuthor.FavoriteSong = authorSong;

            Song reviewerSong = SongFaker.Generate();
            reviewer.FavoriteSong = reviewerSong;

            return authorSong;
        }

        private AuthorChainInstances GetAuthorChainInstances()
        {
            Article article = ArticleFaker.Generate();
            Person author = PersonFaker.Generate();
            article.Author = author;

            List<Blog> blogs = BlogFaker.Generate(2);
            author.Blogs = blogs.ToHashSet();

            blogs[0].Reviewer = author;
            Person reviewer = PersonFaker.Generate();
            blogs[1].Reviewer = reviewer;

            Food authorFood = FoodFaker.Generate();
            author.FavoriteFood = authorFood;
            Food reviewerFood = FoodFaker.Generate();
            reviewer.FavoriteFood = reviewerFood;

            return new AuthorChainInstances(article, author, authorFood, reviewer, reviewerFood);
        }

        private IReadOnlyCollection<RelationshipAttribute> GetIncludedRelationshipsChain(string chain)
        {
            var parsedChain = new List<RelationshipAttribute>();
            ResourceContext resourceContext = ResourceGraph.GetResourceContext<Article>();
            string[] splitPath = chain.Split('.');

            foreach (string requestedRelationship in splitPath)
            {
                RelationshipAttribute relationship =
                    resourceContext.Relationships.Single(nextRelationship => nextRelationship.PublicName == requestedRelationship);

                parsedChain.Add(relationship);
                resourceContext = ResourceGraph.GetResourceContext(relationship.RightType);
            }

            return parsedChain;
        }

        private IncludedResourceObjectBuilder GetBuilder()
        {
            IFieldsToSerialize fields = GetSerializableFields();
            ILinkBuilder links = GetLinkBuilder();

            IResourceDefinitionAccessor accessor = new Mock<IResourceDefinitionAccessor>().Object;

            return new IncludedResourceObjectBuilder(fields, links, ResourceGraph, Enumerable.Empty<IQueryConstraintProvider>(), accessor,
                GetSerializerSettingsProvider());
        }

        private sealed class AuthorChainInstances
        {
            public Article Article { get; }
            public Person Author { get; }
            public Food AuthorFood { get; }
            public Person Reviewer { get; }
            public Food ReviewerFood { get; }

            public AuthorChainInstances(Article article, Person author, Food authorFood, Person reviewer, Food reviewerFood)
            {
                Article = article;
                Author = author;
                AuthorFood = authorFood;
                Reviewer = reviewer;
                ReviewerFood = reviewerFood;
            }

            public void Deconstruct(out Article article, out Person author, out Food authorFood, out Person reviewer, out Food reviewerFood)
            {
                article = Article;
                author = Author;
                authorFood = AuthorFood;
                reviewer = Reviewer;
                reviewerFood = ReviewerFood;
            }
        }
    }
}
