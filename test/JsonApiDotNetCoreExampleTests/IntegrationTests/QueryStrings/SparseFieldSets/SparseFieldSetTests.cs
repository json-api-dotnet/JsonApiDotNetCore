using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.SparseFieldSets
{
    public sealed class SparseFieldSetTests : IClassFixture<ExampleIntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly ExampleIntegrationTestContext<Startup, AppDbContext> _testContext;
        private readonly ExampleFakers _fakers = new ExampleFakers();

        public SparseFieldSetTests(ExampleIntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton<ResourceCaptureStore>();

                services.AddResourceRepository<ResultCapturingRepository<Blog>>();
                services.AddResourceRepository<ResultCapturingRepository<Article>>();
                services.AddResourceRepository<ResultCapturingRepository<Author>>();
                services.AddResourceRepository<ResultCapturingRepository<TodoItem>>();

                services.AddScoped<IResourceService<Article>, JsonApiResourceService<Article>>();
            });
        }

        [Fact]
        public async Task Can_select_fields_in_primary_resources()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?fields[articles]=caption,author";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(article.StringId);
            responseDocument.ManyData[0].Attributes.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(article.Caption);
            responseDocument.ManyData[0].Relationships.Should().HaveCount(1);
            responseDocument.ManyData[0].Relationships["author"].Data.Should().BeNull();
            responseDocument.ManyData[0].Relationships["author"].Links.Self.Should().NotBeNull();
            responseDocument.ManyData[0].Relationships["author"].Links.Related.Should().NotBeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Caption.Should().Be(article.Caption);
            articleCaptured.Url.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_attribute_in_primary_resources()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?fields[articles]=caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(article.StringId);
            responseDocument.ManyData[0].Attributes.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(article.Caption);
            responseDocument.ManyData[0].Relationships.Should().BeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Caption.Should().Be(article.Caption);
            articleCaptured.Url.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_relationship_in_primary_resources()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?fields[articles]=author";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(article.StringId);
            responseDocument.ManyData[0].Attributes.Should().BeNull();
            responseDocument.ManyData[0].Relationships.Should().HaveCount(1);
            responseDocument.ManyData[0].Relationships["author"].Data.Should().BeNull();
            responseDocument.ManyData[0].Relationships["author"].Links.Self.Should().NotBeNull();
            responseDocument.ManyData[0].Relationships["author"].Links.Related.Should().NotBeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Caption.Should().BeNull();
            articleCaptured.Url.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_fields_in_primary_resource_by_ID()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?fields[articles]=url,author";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["url"].Should().Be(article.Url);
            responseDocument.SingleData.Relationships.Should().HaveCount(1);
            responseDocument.SingleData.Relationships["author"].Data.Should().BeNull();
            responseDocument.SingleData.Relationships["author"].Links.Self.Should().NotBeNull();
            responseDocument.SingleData.Relationships["author"].Links.Related.Should().NotBeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Url.Should().Be(article.Url);
            articleCaptured.Caption.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_fields_in_secondary_resources()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var blog = new Blog
            {
                Title = "Some",
                Articles = new List<Article>
                {
                    new Article
                    {
                        Caption = "One",
                        Url = "https://one.domain.com"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/articles?fields[articles]=caption,tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blog.Articles[0].StringId);
            responseDocument.ManyData[0].Attributes.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(blog.Articles[0].Caption);
            responseDocument.ManyData[0].Relationships.Should().HaveCount(1);
            responseDocument.ManyData[0].Relationships["tags"].Data.Should().BeNull();
            responseDocument.ManyData[0].Relationships["tags"].Links.Self.Should().NotBeNull();
            responseDocument.ManyData[0].Relationships["tags"].Links.Related.Should().NotBeNull();

            var blogCaptured = (Blog)store.Resources.Should().ContainSingle(x => x is Blog).And.Subject.Single();
            blogCaptured.Id.Should().Be(blog.Id);
            blogCaptured.Title.Should().BeNull();

            blogCaptured.Articles.Should().HaveCount(1);
            blogCaptured.Articles[0].Caption.Should().Be(blog.Articles[0].Caption);
            blogCaptured.Articles[0].Url.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_fields_of_HasOne_relationship()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = _fakers.Article.Generate();
            article.Caption = "Some";
            article.Author = new Author
            {
                FirstName = "Joe",
                LastName = "Smith",
                BusinessEmail = "nospam@email.com"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?include=author&fields[authors]=lastName,businessEmail,livingAddress";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes["caption"].Should().Be(article.Caption);
            responseDocument.SingleData.Relationships["author"].SingleData.Id.Should().Be(article.Author.StringId);
            responseDocument.SingleData.Relationships["author"].Links.Self.Should().NotBeNull();
            responseDocument.SingleData.Relationships["author"].Links.Related.Should().NotBeNull();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Attributes.Should().HaveCount(2);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(article.Author.LastName);
            responseDocument.Included[0].Attributes["businessEmail"].Should().Be(article.Author.BusinessEmail);
            responseDocument.Included[0].Relationships.Should().HaveCount(1);
            responseDocument.Included[0].Relationships["livingAddress"].Data.Should().BeNull();
            responseDocument.Included[0].Relationships["livingAddress"].Links.Self.Should().NotBeNull();
            responseDocument.Included[0].Relationships["livingAddress"].Links.Related.Should().NotBeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Id.Should().Be(article.Id);
            articleCaptured.Caption.Should().Be(article.Caption);

            articleCaptured.Author.LastName.Should().Be(article.Author.LastName);
            articleCaptured.Author.BusinessEmail.Should().Be(article.Author.BusinessEmail);
            articleCaptured.Author.FirstName.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_fields_of_HasMany_relationship()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var author = _fakers.Author.Generate();
            author.LastName = "Smith";
            author.Articles = new List<Article>
            {
                new Article
                {
                    Caption = "One",
                    Url = "https://one.domain.com"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AuthorDifferentDbContextName.Add(author);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/authors/{author.StringId}?include=articles&fields[articles]=caption,tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(author.StringId);
            responseDocument.SingleData.Attributes["lastName"].Should().Be(author.LastName);
            responseDocument.SingleData.Relationships["articles"].ManyData.Should().HaveCount(1);
            responseDocument.SingleData.Relationships["articles"].ManyData[0].Id.Should().Be(author.Articles[0].StringId);
            responseDocument.SingleData.Relationships["articles"].Links.Self.Should().NotBeNull();
            responseDocument.SingleData.Relationships["articles"].Links.Related.Should().NotBeNull();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Attributes.Should().HaveCount(1);
            responseDocument.Included[0].Attributes["caption"].Should().Be(author.Articles[0].Caption);
            responseDocument.Included[0].Relationships.Should().HaveCount(1);
            responseDocument.Included[0].Relationships["tags"].Data.Should().BeNull();
            responseDocument.Included[0].Relationships["tags"].Links.Self.Should().NotBeNull();
            responseDocument.Included[0].Relationships["tags"].Links.Related.Should().NotBeNull();

            var authorCaptured = (Author) store.Resources.Should().ContainSingle(x => x is Author).And.Subject.Single();
            authorCaptured.Id.Should().Be(author.Id);
            authorCaptured.LastName.Should().Be(author.LastName);

            authorCaptured.Articles.Should().HaveCount(1);
            authorCaptured.Articles[0].Caption.Should().Be(author.Articles[0].Caption);
            authorCaptured.Articles[0].Url.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_fields_of_HasMany_relationship_on_secondary_resource()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var blog = new Blog
            {
                Owner = new Author
                {
                    LastName = "Smith",
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "One",
                            Url = "https://one.domain.com"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/owner?include=articles&fields[articles]=caption,revisions";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(blog.Owner.StringId);
            responseDocument.SingleData.Attributes["lastName"].Should().Be(blog.Owner.LastName);
            responseDocument.SingleData.Relationships["articles"].ManyData.Should().HaveCount(1);
            responseDocument.SingleData.Relationships["articles"].ManyData[0].Id.Should().Be(blog.Owner.Articles[0].StringId);
            responseDocument.SingleData.Relationships["articles"].Links.Self.Should().NotBeNull();
            responseDocument.SingleData.Relationships["articles"].Links.Related.Should().NotBeNull();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Attributes.Should().HaveCount(1);
            responseDocument.Included[0].Attributes["caption"].Should().Be(blog.Owner.Articles[0].Caption);
            responseDocument.Included[0].Relationships.Should().HaveCount(1);
            responseDocument.Included[0].Relationships["revisions"].Data.Should().BeNull();
            responseDocument.Included[0].Relationships["revisions"].Links.Self.Should().NotBeNull();
            responseDocument.Included[0].Relationships["revisions"].Links.Related.Should().NotBeNull();

            var blogCaptured = (Blog) store.Resources.Should().ContainSingle(x => x is Blog).And.Subject.Single();
            blogCaptured.Id.Should().Be(blog.Id);
            blogCaptured.Owner.Should().NotBeNull();
            blogCaptured.Owner.LastName.Should().Be(blog.Owner.LastName);

            blogCaptured.Owner.Articles.Should().HaveCount(1);
            blogCaptured.Owner.Articles[0].Caption.Should().Be(blog.Owner.Articles[0].Caption);
            blogCaptured.Owner.Articles[0].Url.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_fields_of_HasManyThrough_relationship()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = _fakers.Article.Generate();
            article.Caption = "Some";
            article.ArticleTags = new HashSet<ArticleTag>
            {
                new ArticleTag
                {
                    Tag = new Tag
                    {
                        Name = "Hot"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?include=tags&fields[tags]=color";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes["caption"].Should().Be(article.Caption);
            responseDocument.SingleData.Relationships["tags"].ManyData.Should().HaveCount(1);
            responseDocument.SingleData.Relationships["tags"].ManyData[0].Id.Should().Be(article.ArticleTags.ElementAt(0).Tag.StringId);
            responseDocument.SingleData.Relationships["tags"].Links.Self.Should().NotBeNull();
            responseDocument.SingleData.Relationships["tags"].Links.Related.Should().NotBeNull();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Attributes.Should().HaveCount(1);
            responseDocument.Included[0].Attributes["color"].Should().Be(article.ArticleTags.Single().Tag.Color.ToString("G"));
            responseDocument.Included[0].Relationships.Should().BeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Id.Should().Be(article.Id);
            articleCaptured.Caption.Should().Be(article.Caption);

            articleCaptured.ArticleTags.Should().HaveCount(1);
            articleCaptured.ArticleTags.Single().Tag.Color.Should().Be(article.ArticleTags.Single().Tag.Color);
            articleCaptured.ArticleTags.Single().Tag.Name.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_attributes_in_multiple_resource_types()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var blog = new Blog
            {
                Title = "Technology",
                CompanyName = "Contoso",
                Owner = new Author
                {
                    FirstName = "Jason",
                    LastName = "Smith",
                    DateOfBirth = 21.November(1999),
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "One",
                            Url = "www.one.com"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}?include=owner.articles&fields[blogs]=title&fields[authors]=firstName,lastName&fields[articles]=caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(blog.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["title"].Should().Be(blog.Title);
            responseDocument.SingleData.Relationships.Should().BeNull();

            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Id.Should().Be(blog.Owner.StringId);
            responseDocument.Included[0].Attributes.Should().HaveCount(2);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(blog.Owner.FirstName);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(blog.Owner.LastName);
            responseDocument.Included[0].Relationships.Should().BeNull();

            responseDocument.Included[1].Id.Should().Be(blog.Owner.Articles[0].StringId);
            responseDocument.Included[1].Attributes.Should().HaveCount(1);
            responseDocument.Included[1].Attributes["caption"].Should().Be(blog.Owner.Articles[0].Caption);
            responseDocument.Included[1].Relationships.Should().BeNull();

            var blogCaptured = (Blog) store.Resources.Should().ContainSingle(x => x is Blog).And.Subject.Single();
            blogCaptured.Id.Should().Be(blog.Id);
            blogCaptured.Title.Should().Be(blog.Title);
            blogCaptured.CompanyName.Should().BeNull();

            blogCaptured.Owner.FirstName.Should().Be(blog.Owner.FirstName);
            blogCaptured.Owner.LastName.Should().Be(blog.Owner.LastName);
            blogCaptured.Owner.DateOfBirth.Should().BeNull();

            blogCaptured.Owner.Articles.Should().HaveCount(1);
            blogCaptured.Owner.Articles[0].Caption.Should().Be(blog.Owner.Articles[0].Caption);
            blogCaptured.Owner.Articles[0].Url.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_only_top_level_fields_with_multiple_includes()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var blog = new Blog
            {
                Title = "Technology",
                CompanyName = "Contoso",
                Owner = new Author
                {
                    FirstName = "Jason",
                    LastName = "Smith",
                    DateOfBirth = 21.November(1999),
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "One",
                            Url = "www.one.com"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}?include=owner.articles&fields[blogs]=title,owner";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(blog.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["title"].Should().Be(blog.Title);
            responseDocument.SingleData.Relationships.Should().HaveCount(1);
            responseDocument.SingleData.Relationships["owner"].SingleData.Id.Should().Be(blog.Owner.StringId);
            responseDocument.SingleData.Relationships["owner"].Links.Self.Should().NotBeNull();
            responseDocument.SingleData.Relationships["owner"].Links.Related.Should().NotBeNull();

            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Id.Should().Be(blog.Owner.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(blog.Owner.FirstName);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(blog.Owner.LastName);
            responseDocument.Included[0].Attributes["dateOfBirth"].Should().BeCloseTo(blog.Owner.DateOfBirth);
            responseDocument.Included[0].Relationships["articles"].ManyData.Should().HaveCount(1);
            responseDocument.Included[0].Relationships["articles"].ManyData[0].Id.Should().Be(blog.Owner.Articles[0].StringId);
            responseDocument.Included[0].Relationships["articles"].Links.Self.Should().NotBeNull();
            responseDocument.Included[0].Relationships["articles"].Links.Related.Should().NotBeNull();

            responseDocument.Included[1].Id.Should().Be(blog.Owner.Articles[0].StringId);
            responseDocument.Included[1].Attributes["caption"].Should().Be(blog.Owner.Articles[0].Caption);
            responseDocument.Included[1].Attributes["url"].Should().Be(blog.Owner.Articles[0].Url);
            responseDocument.Included[1].Relationships["tags"].Data.Should().BeNull();
            responseDocument.Included[1].Relationships["tags"].Links.Self.Should().NotBeNull();
            responseDocument.Included[1].Relationships["tags"].Links.Related.Should().NotBeNull();

            var blogCaptured = (Blog) store.Resources.Should().ContainSingle(x => x is Blog).And.Subject.Single();
            blogCaptured.Id.Should().Be(blog.Id);
            blogCaptured.Title.Should().Be(blog.Title);
            blogCaptured.CompanyName.Should().BeNull();
        }

        [Fact]
        public async Task Can_select_ID()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?fields[articles]=id,caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(article.StringId);
            responseDocument.ManyData[0].Attributes.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(article.Caption);
            responseDocument.ManyData[0].Relationships.Should().BeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Id.Should().Be(article.Id);
            articleCaptured.Caption.Should().Be(article.Caption);
            articleCaptured.Url.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_select_on_unknown_resource_type()
        {
            // Arrange
            var route = "/api/v1/people?fields[doesNotExist]=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified fieldset is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Resource type 'doesNotExist' does not exist.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("fields[doesNotExist]");
        }

        [Fact]
        public async Task Cannot_select_attribute_with_blocked_capability()
        {
            // Arrange
            var user = _fakers.User.Generate();

            var route = $"/api/v1/users/{user.Id}?fields[users]=password";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Retrieving the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Retrieving the attribute 'password' is not allowed.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("fields[users]");
        }

        [Fact]
        public async Task Retrieves_all_properties_when_fieldset_contains_readonly_attribute()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var todoItem = new TodoItem
            {
                Description = "Pending work..."
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/todoItems/{todoItem.StringId}?fields[todoItems]=calculatedValue";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(todoItem.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["calculatedValue"].Should().Be(todoItem.CalculatedValue);
            responseDocument.SingleData.Relationships.Should().BeNull();

            var todoItemCaptured = (TodoItem) store.Resources.Should().ContainSingle(x => x is TodoItem).And.Subject.Single();
            todoItemCaptured.CalculatedValue.Should().Be(todoItem.CalculatedValue);
            todoItemCaptured.Description.Should().Be(todoItem.Description);
        }

        [Fact]
        public async Task Can_select_fields_on_resource_type_multiple_times()
        {
            // Arrange
            var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
            store.Clear();

            var article = new Article
            {
                Caption = "One",
                Url = "https://one.domain.com",
                Author = new Author
                {
                    LastName = "Smith"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?fields[articles]=url&fields[articles]=caption,url&fields[articles]=caption,author";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(2);
            responseDocument.SingleData.Attributes["caption"].Should().Be(article.Caption);
            responseDocument.SingleData.Attributes["url"].Should().Be(article.Url);
            responseDocument.SingleData.Relationships.Should().HaveCount(1);
            responseDocument.SingleData.Relationships["author"].Data.Should().BeNull();
            responseDocument.SingleData.Relationships["author"].Links.Self.Should().NotBeNull();
            responseDocument.SingleData.Relationships["author"].Links.Related.Should().NotBeNull();

            var articleCaptured = (Article) store.Resources.Should().ContainSingle(x => x is Article).And.Subject.Single();
            articleCaptured.Id.Should().Be(article.Id);
            articleCaptured.Caption.Should().Be(article.Caption);
            articleCaptured.Url.Should().Be(articleCaptured.Url);
        }
    }
}
