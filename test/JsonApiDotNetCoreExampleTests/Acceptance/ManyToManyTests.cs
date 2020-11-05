using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    // TODO: Move left-over tests in this file.

    public sealed class ManyToManyTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        private readonly Faker<Author> _authorFaker;
        private readonly Faker<Article> _articleFaker;
        private readonly Faker<Tag> _tagFaker;

        public ManyToManyTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            _authorFaker = new Faker<Author>()
                .RuleFor(a => a.LastName, f => f.Random.Words(2));

            _articleFaker = new Faker<Article>()
                .RuleFor(a => a.Caption, f => f.Random.AlphaNumeric(10))
                .RuleFor(a => a.Author, f => _authorFaker.Generate());

            _tagFaker = new Faker<Tag>()
                .CustomInstantiator(f => new Tag())
                .RuleFor(a => a.Name, f => f.Random.AlphaNumeric(10));
        }

        [Fact]
        public async Task Can_Get_HasManyThrough_Relationship_Through_Secondary_Endpoint()
        {
            // Arrange
            var existingArticleTag = new ArticleTag
            {
                Article = _articleFaker.Generate(),
                Tag = _tagFaker.Generate()
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.ArticleTags.Add(existingArticleTag);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{existingArticleTag.Article.StringId}/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Type.Should().Be("tags");
            responseDocument.ManyData[0].Id.Should().Be(existingArticleTag.Tag.StringId);
            responseDocument.ManyData[0].Attributes["name"].Should().Be(existingArticleTag.Tag.Name);
        }

        [Fact]
        public async Task Can_Get_HasManyThrough_Through_Relationship_Endpoint()
        {
            // Arrange
            var existingArticleTag = new ArticleTag
            {
                Article = _articleFaker.Generate(),
                Tag = _tagFaker.Generate()
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.ArticleTags.Add(existingArticleTag);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{existingArticleTag.Article.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Type.Should().Be("tags");
            responseDocument.ManyData[0].Id.Should().Be(existingArticleTag.Tag.StringId);
            responseDocument.ManyData[0].Attributes.Should().BeNull();
        }
    }
}
