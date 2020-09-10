using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Acceptance.Spec;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public sealed class ModelStateValidationTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        private readonly Faker<Article> _articleFaker;
        private readonly Faker<Author> _authorFaker;
        private readonly Faker<Tag> _tagFaker;

        public ModelStateValidationTests(StandardApplicationFactory factory)
            : base(factory)
        {
            var options = (JsonApiOptions) _factory.GetService<IJsonApiOptions>();
            options.ValidateModelState = true;

            var context = _factory.GetService<AppDbContext>();

            _authorFaker = new Faker<Author>()
                .RuleFor(a => a.LastName, f => f.Random.Words(2));

            _articleFaker = new Faker<Article>()
                .RuleFor(a => a.Caption, f => f.Random.AlphaNumeric(10))
                .RuleFor(a => a.Author, f => _authorFaker.Generate());

            _tagFaker = new Faker<Tag>()
                .RuleFor(a => a.Name, f => f.Random.AlphaNumeric(10));
        }

        [Fact]
        public async Task When_posting_tag_with_invalid_name_it_must_fail()
        {
            // Arrange
            var tag = new Tag
            {
                Name = "!@#$%^&*().-"
            };

            var serializer = GetSerializer<Tag>();
            var content = serializer.Serialize(tag);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tags")
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Input validation failed.", errorDocument.Errors[0].Title);
            Assert.Equal("The field Name must match the regular expression '^\\W$'.", errorDocument.Errors[0].Detail);
            Assert.Equal("/data/attributes/name", errorDocument.Errors[0].Source.Pointer);
        }

        [Fact]
        public async Task When_posting_tag_with_invalid_name_without_model_state_validation_it_must_succeed()
        {
            // Arrange
            var tag = new Tag
            {
                Name = "!@#$%^&*().-"
            };

            var serializer = GetSerializer<Tag>();
            var content = serializer.Serialize(tag);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tags")
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            var options = (JsonApiOptions)_factory.GetService<IJsonApiOptions>();
            options.ValidateModelState = false;

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task When_patching_tag_with_invalid_name_it_must_fail()
        {
            // Arrange
            var existingTag = new Tag
            {
                Name = "Technology"
            };

            var context = _factory.GetService<AppDbContext>();
            context.Tags.Add(existingTag);
            await context.SaveChangesAsync();

            var updatedTag = new Tag
            {
                Id = existingTag.Id,
                Name = "!@#$%^&*().-"
            };

            var serializer = GetSerializer<Tag>();
            var content = serializer.Serialize(updatedTag);

            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/tags/" + existingTag.StringId)
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Input validation failed.", errorDocument.Errors[0].Title);
            Assert.Equal("The field Name must match the regular expression '^\\W$'.", errorDocument.Errors[0].Detail);
            Assert.Equal("/data/attributes/name", errorDocument.Errors[0].Source.Pointer);
        }

        [Fact]
        public async Task When_patching_tag_with_invalid_name_without_model_state_validation_it_must_succeed()
        {
            // Arrange
            var existingTag = new Tag
            {
                Name = "Technology"
            };

            var context = _factory.GetService<AppDbContext>();
            context.Tags.Add(existingTag);
            await context.SaveChangesAsync();

            var updatedTag = new Tag
            {
                Id = existingTag.Id,
                Name = "!@#$%^&*().-"
            };

            var serializer = GetSerializer<Tag>();
            var content = serializer.Serialize(updatedTag);

            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/tags/" + existingTag.StringId)
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            var options = (JsonApiOptions)_factory.GetService<IJsonApiOptions>();
            options.ValidateModelState = false;

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_Article_With_IsRequired_Name_Attribute_Succeeds()
        {
            // Arrange
            string name = "Article Title";
            var context = _factory.GetService<AppDbContext>();
            var author = _authorFaker.Generate();
            context.AuthorDifferentDbContextName.Add(author);
            await context.SaveChangesAsync();

            var route = "/api/v1/articles";
            var request = new HttpRequestMessage(HttpMethod.Post, route);
            var content = new
            {
                data = new
                {
                    type = "articles",
                    attributes = new Dictionary<string, object>
                    {
                        {"caption", name}
                    },
                    relationships = new Dictionary<string, dynamic>
                    {
                        {  "author",  new
                            {
                                data = new
                                {
                                    type = "authors",
                                    id = author.StringId
                                }
                             }
                        }
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var articleResponse = GetDeserializer().DeserializeSingle<Article>(body).Data;
            Assert.NotNull(articleResponse);

            var persistedArticle = await _dbContext.Articles
                .SingleAsync(a => a.Id == articleResponse.Id);

            Assert.Equal(name, persistedArticle.Caption);
        }

        [Fact]
        public async Task Create_Article_With_IsRequired_Name_Attribute_Empty_Succeeds()
        {
            // Arrange
            string name = string.Empty;
            var context = _factory.GetService<AppDbContext>();
            var author = _authorFaker.Generate();
            context.AuthorDifferentDbContextName.Add(author);
            await context.SaveChangesAsync();

            var route = "/api/v1/articles";
            var request = new HttpRequestMessage(HttpMethod.Post, route);
            var content = new
            {
                data = new
                {
                    type = "articles",
                    attributes = new Dictionary<string, object>
                    {
                        {"caption", name}
                    },
                    relationships = new Dictionary<string, dynamic>
                    {
                        {  "author",  new
                            {
                                data = new
                                {
                                    type = "authors",
                                    id = author.StringId
                                }
                             }
                        }
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var articleResponse = GetDeserializer().DeserializeSingle<Article>(body).Data;
            Assert.NotNull(articleResponse);

            var persistedArticle = await _dbContext.Articles
                .SingleAsync(a => a.Id == articleResponse.Id);

            Assert.Equal(name, persistedArticle.Caption);
        }

        [Fact]
        public async Task Create_Article_With_IsRequired_Name_Attribute_Explicitly_Null_Fails()
        {
            // Arrange
            var context = _factory.GetService<AppDbContext>();
            var author = _authorFaker.Generate();
            context.AuthorDifferentDbContextName.Add(author);
            await context.SaveChangesAsync();

            var route = "/api/v1/articles";
            var request = new HttpRequestMessage(HttpMethod.Post, route);
            var content = new
            {
                data = new
                {
                    type = "articles",
                    attributes = new Dictionary<string, object>
                    {
                        {"caption", null}
                    },
                    relationships = new Dictionary<string, dynamic>
                    {
                        {  "author",  new
                            {
                                data = new
                                {
                                    type = "authors",
                                    id = author.StringId
                                }
                            }
                        }
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal("Input validation failed.", errorDocument.Errors[0].Title);
            Assert.Equal("422", errorDocument.Errors[0].Status);
            Assert.Equal("The Caption field is required.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Create_Article_With_IsRequired_Name_Attribute_Missing_Fails()
        {
            // Arrange
            var context = _factory.GetService<AppDbContext>();
            var author = _authorFaker.Generate();
            context.AuthorDifferentDbContextName.Add(author);
            await context.SaveChangesAsync();

            var route = "/api/v1/articles";
            var request = new HttpRequestMessage(HttpMethod.Post, route);
            var content = new
            {
                data = new
                {
                    type = "articles",
                    relationships = new Dictionary<string, dynamic>
                    {
                        {  "author",  new
                            {
                                data = new
                                {
                                    type = "authors",
                                    id = author.StringId
                                }
                            }
                        }
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal("Input validation failed.", errorDocument.Errors[0].Title);
            Assert.Equal("422", errorDocument.Errors[0].Status);
            Assert.Equal("The Caption field is required.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Update_Article_With_IsRequired_Name_Attribute_Succeeds()
        {
            // Arrange
            var name = "Article Name";
            var context = _factory.GetService<AppDbContext>();
            var article = _articleFaker.Generate();
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}";
            var request = new HttpRequestMessage(HttpMethod.Patch, route);
            var content = new
            {
                data = new
                {
                    type = "articles",
                    id = article.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        {"caption", name}
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var persistedArticle = await _dbContext.Articles
                .SingleOrDefaultAsync(a => a.Id == article.Id);

            var updatedName = persistedArticle.Caption;
            Assert.Equal(name, updatedName);
        }

        [Fact]
        public async Task Update_Article_With_IsRequired_Name_Attribute_Missing_Succeeds()
        {
            // Arrange
            var context = _factory.GetService<AppDbContext>();
            var tag = _tagFaker.Generate();
            var article = _articleFaker.Generate();
            context.Tags.Add(tag);
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}";
            var request = new HttpRequestMessage(HttpMethod.Patch, route);
            var content = new
            {
                data = new
                {
                    type = "articles",
                    id = article.StringId,
                    relationships = new Dictionary<string, dynamic>
                    {
                        {  "tags",  new
                            {
                                data = new []
                                {
                                    new
                                    {
                                        type = "tags",
                                        id = tag.StringId
                                    }
                                }
                            }
                        }
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Update_Article_With_IsRequired_Name_Attribute_Explicitly_Null_Fails()
        {
            // Arrange
            var context = _factory.GetService<AppDbContext>();
            var article = _articleFaker.Generate();
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}";
            var request = new HttpRequestMessage(HttpMethod.Patch, route);
            var content = new
            {
                data = new
                {
                    type = "articles",
                    id = article.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        {"caption", null}
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal("Input validation failed.", errorDocument.Errors[0].Title);
            Assert.Equal("422", errorDocument.Errors[0].Status);
            Assert.Equal("The Caption field is required.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Update_Article_With_IsRequired_Name_Attribute_Empty_Succeeds()
        {
            // Arrange
            var context = _factory.GetService<AppDbContext>();
            var article = _articleFaker.Generate();
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}";
            var request = new HttpRequestMessage(HttpMethod.Patch, route);
            var content = new
            {
                data = new
                {
                    type = "articles",
                    id = article.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        {"caption", ""}
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var persistedArticle = await _dbContext.Articles
                .SingleOrDefaultAsync(a => a.Id == article.Id);

            var updatedName = persistedArticle.Caption;
            Assert.Equal("", updatedName);
        }
    }
}
