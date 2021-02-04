using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Client.Internal;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Definitions;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks
{
    public sealed class ResourceHookTests
        : IClassFixture<ExampleIntegrationTestContext<ResourceHooksStartup<AppDbContext>, AppDbContext>>
    {
        private readonly ExampleIntegrationTestContext<ResourceHooksStartup<AppDbContext>, AppDbContext> _testContext;
        private readonly ExampleFakers _fakers;

        public ResourceHookTests(ExampleIntegrationTestContext<ResourceHooksStartup<AppDbContext>, AppDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped<ResourceHooksDefinition<Article>, ArticleHooksDefinition>();
                services.AddScoped<ResourceHooksDefinition<Passport>, PassportHooksDefinition>();
                services.AddScoped<ResourceHooksDefinition<Person>, PersonHooksDefinition>();
                services.AddScoped<ResourceHooksDefinition<Tag>, TagHooksDefinition>();
                services.AddScoped<ResourceHooksDefinition<TodoItem>, TodoItemHooksDefinition>();
            });

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = false;
            options.DisableChildrenPagination = false;

            _fakers = new ExampleFakers(testContext.Factory.Services);
        }

        [Fact]
        public async Task Can_create_user_with_password()
        {
            // Arrange
            var user = _fakers.User.Generate();

            var serializer = GetRequestSerializer<User>(p => new {p.Password, p.UserName});

            var route = "/api/v1/users";

            var request = new HttpRequestMessage(HttpMethod.Post, route)
            {
                Content = new StringContent(serializer.Serialize(user))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Created);

            var body = await response.Content.ReadAsStringAsync();
            var returnedUser = GetResponseDeserializer().DeserializeSingle<User>(body).Data;
            var document = JsonConvert.DeserializeObject<Document>(body);

            document.SingleData.Attributes.Should().NotContainKey("password");
            document.SingleData.Attributes["userName"].Should().Be(user.UserName);

            using var scope = _testContext.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbUser = await dbContext.Users.FindAsync(returnedUser.Id);

            dbUser.UserName.Should().Be(user.UserName);
            dbUser.Password.Should().Be(user.Password);
        }

        [Fact]
        public async Task Can_update_user_password()
        {
            // Arrange
            var user = _fakers.User.Generate();

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            }

            user.Password = _fakers.User.Generate().Password;

            var serializer = GetRequestSerializer<User>(p => new {p.Password});

            var route = $"/api/v1/users/{user.Id}";

            var request = new HttpRequestMessage(HttpMethod.Patch, route)
            {
                Content = new StringContent(serializer.Serialize(user))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);

            document.SingleData.Attributes.Should().NotContainKey("password");
            document.SingleData.Attributes["userName"].Should().Be(user.UserName);

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var dbUser = dbContext.Users.Single(u => u.Id == user.Id);

                dbUser.Password.Should().Be(user.Password);
            }
        }

        [Fact]
        public async Task Unauthorized_TodoItem()
        {
            // Arrange
            var route = "/api/v1/todoItems/1337";

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.GetAsync(route);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to update the author of todo items.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Unauthorized_Passport()
        {
            // Arrange
            var route = "/api/v1/people/1?include=passport";

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.GetAsync(route);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to include passports on individual persons.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Unauthorized_Article()
        {
            // Arrange
            var article = _fakers.Article.Generate();
            article.Caption = "Classified";

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Articles.Add(article);
                await dbContext.SaveChangesAsync();
            }

            var route = $"/api/v1/articles/{article.Id}";

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.GetAsync(route);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to see this article.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Article_is_hidden()
        {
            // Arrange
            var articles = _fakers.Article.Generate(3);
            string toBeExcluded = "This should not be included";
            articles[0].Caption = toBeExcluded;

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Articles.AddRange(articles);
                await dbContext.SaveChangesAsync();
            }

            var route = "/api/v1/articles";

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.GetAsync(route);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Article_through_secondary_endpoint_is_hidden()
        {
            // Arrange
            var articles = _fakers.Article.Generate(3);
            string toBeExcluded = "This should not be included";
            articles[0].Caption = toBeExcluded;

            var author = _fakers.Author.Generate();
            author.Articles = articles;

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.AuthorDifferentDbContextName.Add(author);
                await dbContext.SaveChangesAsync();
            }

            var route = $"/api/v1/authors/{author.Id}/articles";

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.GetAsync(route);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Tag_is_hidden()
        {
            // Arrange
            var article = _fakers.Article.Generate();
            var tags = _fakers.Tag.Generate(2);
            string toBeExcluded = "This should not be included";
            tags[0].Name = toBeExcluded;

            var articleTags = new[]
            {
                new ArticleTag
                {
                    Article = article,
                    Tag = tags[0]
                },
                new ArticleTag
                {
                    Article = article,
                    Tag = tags[1]
                }
            };

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.ArticleTags.AddRange(articleTags);
                await dbContext.SaveChangesAsync();
            }

            // Workaround for https://github.com/dotnet/efcore/issues/21026
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = false;
            options.DisableChildrenPagination = true;

            var route = "/api/v1/articles?include=tags";

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.GetAsync(route);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Cascade_permission_error_create_ToOne_relationship()
        {
            // In the Cascade Permission Error tests, we ensure that all the relevant resources are provided in the hook definitions. In this case, 
            // re-relating the meta object to a different article would require also a check for the lockedTodo, because we're implicitly updating 
            // its foreign key.

            // Arrange
            var lockedPerson = _fakers.Person.Generate();
            lockedPerson.IsLocked = true;

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var passport = new Passport(dbContext);
                lockedPerson.Passport = passport;

                dbContext.People.Add(lockedPerson);
                await dbContext.SaveChangesAsync();
            }

            var requestBody = new
            {
                data = new
                {
                    type = "people",
                    relationships = new
                    {
                        passport = new
                        {
                            data = new
                            {
                                type = "passports",
                                id = lockedPerson.Passport.StringId
                            }
                        }
                    }
                }
            };

            var route = "/api/v1/people";

            var request = new HttpRequestMessage(HttpMethod.Post, route);

            string requestText = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(requestText);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked todo items.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cascade_permission_error_updating_ToOne_relationship()
        {
            // Arrange
            var person = _fakers.Person.Generate();
            Passport newPassport;

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var passport = new Passport(dbContext) {IsLocked = true};
                person.Passport = passport;
                dbContext.People.Add(person);
                newPassport = new Passport(dbContext);
                dbContext.Passports.Add(newPassport);
                await dbContext.SaveChangesAsync();
            }

            var requestBody = new
            {
                data = new
                {
                    type = "people",
                    id = person.Id,
                    relationships = new
                    {
                        passport = new
                        {
                            data = new
                            {
                                type = "passports",
                                id = newPassport.StringId
                            }
                        }
                    }
                }
            };

            var route = $"/api/v1/people/{person.Id}";

            var request = new HttpRequestMessage(HttpMethod.Patch, route);

            string requestText = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(requestText);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            
            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked persons.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cascade_permission_error_updating_ToOne_relationship_deletion()
        {
            // Arrange
            var person = _fakers.Person.Generate();

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var passport = new Passport(dbContext) {IsLocked = true};
                person.Passport = passport;
                dbContext.People.Add(person);
                var newPassport = new Passport(dbContext);
                dbContext.Passports.Add(newPassport);
                await dbContext.SaveChangesAsync();
            }

            var requestBody = new
            {
                data = new
                {
                    type = "people",
                    id = person.Id,
                    relationships = new
                    {
                        passport = new
                        {
                            data = (object) null
                        }
                    }
                }
            };

            var route = $"/api/v1/people/{person.Id}";

            var request = new HttpRequestMessage(HttpMethod.Patch, route);

            string requestText = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(requestText);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked persons.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cascade_permission_error_delete_ToOne_relationship()
        {
            // Arrange
            var lockedPerson = _fakers.Person.Generate();
            lockedPerson.IsLocked = true;

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var passport = new Passport(dbContext);
                lockedPerson.Passport = passport;
                dbContext.People.Add(lockedPerson);
                await dbContext.SaveChangesAsync();
            }

            var route = $"/api/v1/passports/{lockedPerson.Passport.StringId}";

            var request = new HttpRequestMessage(HttpMethod.Delete, route);

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked todo items.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cascade_permission_error_create_ToMany_relationship()
        {
            // Arrange
            var persons = _fakers.Person.Generate(2);
            var lockedTodo = _fakers.TodoItem.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.TodoItems.Add(lockedTodo);
                await dbContext.SaveChangesAsync();
            }

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    relationships = new
                    {
                        stakeHolders = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "people",
                                    id = persons[0].StringId
                                },
                                new
                                {
                                    type = "people",
                                    id = persons[1].StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/api/v1/todoItems";

            var request = new HttpRequestMessage(HttpMethod.Post, route);

            string requestText = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(requestText);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked todo items.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cascade_permission_error_updating_ToMany_relationship()
        {
            // Arrange
            var persons = _fakers.Person.Generate(2);

            var lockedTodo = _fakers.TodoItem.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();

            var unlockedTodo = _fakers.TodoItem.Generate();

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.TodoItems.AddRange(lockedTodo, unlockedTodo);
                await dbContext.SaveChangesAsync();
            }

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    id = unlockedTodo.Id,
                    relationships = new
                    {
                        stakeHolders = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "people",
                                    id = persons[0].StringId
                                },
                                new
                                {
                                    type = "people",
                                    id = persons[1].StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = $"/api/v1/todoItems/{unlockedTodo.Id}";

            var request = new HttpRequestMessage(HttpMethod.Patch, route);

            string requestText = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(requestText);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked todo items.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cascade_permission_error_delete_ToMany_relationship()
        {
            // Arrange
            var persons = _fakers.Person.Generate(2);
            var lockedTodo = _fakers.TodoItem.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();

            using (var scope = _testContext.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.TodoItems.Add(lockedTodo);
                await dbContext.SaveChangesAsync();
            }

            var route = $"/api/v1/people/{persons[0].Id}";

            var request = new HttpRequestMessage(HttpMethod.Delete, route);

            using var client = _testContext.Factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            errorDocument.Errors.Should().HaveCount(1);
            errorDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            errorDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked todo items.");
            errorDocument.Errors[0].Detail.Should().BeNull();
        }

        private IRequestSerializer GetRequestSerializer<TResource>(Expression<Func<TResource, dynamic>> attributes = null,
            Expression<Func<TResource, dynamic>> relationships = null)
            where TResource : class, IIdentifiable
        {
            var graph = _testContext.Factory.Services.GetRequiredService<IResourceGraph>();

            var serializer = _testContext.Factory.Services.GetRequiredService<IRequestSerializer>();
            serializer.AttributesToSerialize = attributes != null ? graph.GetAttributes(attributes) : null;
            serializer.RelationshipsToSerialize = relationships != null ? graph.GetRelationships(relationships) : null;
            return serializer;
        }

        private IResponseDeserializer GetResponseDeserializer()
        {
            return _testContext.Factory.Services.GetRequiredService<IResponseDeserializer>();
        }
    }
}
