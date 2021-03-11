using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
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
    public sealed class ResourceHookTests : IClassFixture<ExampleIntegrationTestContext<ResourceHooksStartup<AppDbContext>, AppDbContext>>
    {
        private readonly ExampleIntegrationTestContext<ResourceHooksStartup<AppDbContext>, AppDbContext> _testContext;
        private readonly ExampleFakers _fakers = new ExampleFakers();

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

                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });

            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = false;
            options.DisableChildrenPagination = false;
        }

        [Fact]
        public async Task Can_create_user_with_password()
        {
            // Arrange
            User newUser = _fakers.User.Generate();

            IRequestSerializer serializer = GetRequestSerializer<User>(user => new
            {
                user.Password,
                user.UserName
            });

            string requestBody = serializer.Serialize(newUser);

            const string route = "/api/v1/users";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            User responseUser = GetResponseDeserializer().DeserializeSingle<User>(responseDocument).Data;
            var document = JsonConvert.DeserializeObject<Document>(responseDocument);

            document.SingleData.Attributes.Should().NotContainKey("password");
            document.SingleData.Attributes["userName"].Should().Be(newUser.UserName);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                User userInDatabase = await dbContext.Users.FirstWithIdAsync(responseUser.Id);

                userInDatabase.UserName.Should().Be(newUser.UserName);
                userInDatabase.Password.Should().Be(newUser.Password);
            });
        }

        [Fact]
        public async Task Can_update_user_password()
        {
            // Arrange
            User existingUser = _fakers.User.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Users.Add(existingUser);
                await dbContext.SaveChangesAsync();
            });

            existingUser.Password = _fakers.User.Generate().Password;

            IRequestSerializer serializer = GetRequestSerializer<User>(user => new
            {
                user.Password
            });

            string requestBody = serializer.Serialize(existingUser);

            string route = $"/api/v1/users/{existingUser.Id}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Attributes.Should().NotContainKey("password");
            responseDocument.SingleData.Attributes["userName"].Should().Be(existingUser.UserName);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                User userInDatabase = await dbContext.Users.FirstWithIdAsync(existingUser.Id);

                userInDatabase.Password.Should().Be(existingUser.Password);
            });
        }

        [Fact]
        public async Task Can_block_access_to_resource_from_GetSingle_endpoint_using_BeforeRead_hook()
        {
            // Arrange
            const string route = "/api/v1/todoItems/1337";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to update the author of todo items.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_access_to_included_resource_using_BeforeRead_hook()
        {
            // Arrange
            const string route = "/api/v1/people/1?include=passport";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to include passports on individual persons.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_access_to_resource_from_GetSingle_endpoint_using_OnReturn_hook()
        {
            // Arrange
            Article article = _fakers.Article.Generate();
            article.Caption = "Classified";

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/v1/articles/{article.Id}";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to see this article.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_hide_primary_resource_from_result_set_from_GetAll_endpoint_using_OnReturn_hook()
        {
            // Arrange
            List<Article> articles = _fakers.Article.Generate(3);
            const string toBeExcluded = "This should not be included";
            articles[0].Caption = toBeExcluded;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.AddRange(articles);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/api/v1/articles";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Can_hide_secondary_resource_from_ToOne_relationship_using_OnReturn_hook()
        {
            // Arrange
            Person person = _fakers.Person.Generate();

            person.Passport = new Passport
            {
                IsLocked = true
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(person);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/v1/people/{person.Id}/passport";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }

        [Fact]
        public async Task Can_hide_secondary_resource_from_ToMany_List_relationship_using_OnReturn_hook()
        {
            // Arrange
            const string toBeExcluded = "This should not be included";

            Author author = _fakers.Author.Generate();
            author.Articles = _fakers.Article.Generate(3);
            author.Articles[0].Caption = toBeExcluded;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AuthorDifferentDbContextName.Add(author);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/v1/authors/{author.Id}/articles";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Can_hide_secondary_resource_from_ToMany_Set_relationship_using_OnReturn_hook()
        {
            // Arrange
            const string toBeExcluded = "This should not be included";

            Person person = _fakers.Person.Generate();
            person.TodoItems = _fakers.TodoItem.Generate(3).ToHashSet();
            person.TodoItems.First().Description = toBeExcluded;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(person);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/v1/people/{person.Id}/todoItems";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Can_hide_resource_from_included_HasManyThrough_relationship_using_OnReturn_hook()
        {
            // Arrange
            const string toBeExcluded = "This should not be included";

            List<Tag> tags = _fakers.Tag.Generate(2);
            tags[0].Name = toBeExcluded;

            Article article = _fakers.Article.Generate();

            article.ArticleTags = new HashSet<ArticleTag>
            {
                new ArticleTag
                {
                    Tag = tags[0]
                },
                new ArticleTag
                {
                    Tag = tags[1]
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);
                await dbContext.SaveChangesAsync();
            });

            // Workaround for https://github.com/dotnet/efcore/issues/21026
            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = false;
            options.DisableChildrenPagination = true;

            const string route = "/api/v1/articles?include=tags";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Can_block_creating_ToOne_relationship_using_BeforeUpdateRelationship_hook()
        {
            // Arrange
            Person lockedPerson = _fakers.Person.Generate();
            lockedPerson.IsLocked = true;
            lockedPerson.Passport = new Passport();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(lockedPerson);
                await dbContext.SaveChangesAsync();
            });

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

            const string route = "/api/v1/people";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'people'.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_replacing_ToOne_relationship_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            Person person = _fakers.Person.Generate();

            person.Passport = new Passport
            {
                IsLocked = true
            };

            var newPassport = new Passport();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(person, newPassport);
                await dbContext.SaveChangesAsync();
            });

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

            string route = $"/api/v1/people/{person.Id}";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'passports'.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_clearing_ToOne_relationship_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            Person person = _fakers.Person.Generate();

            person.Passport = new Passport
            {
                IsLocked = true
            };

            var newPassport = new Passport();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(person, newPassport);
                await dbContext.SaveChangesAsync();
            });

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
                            data = (object)null
                        }
                    }
                }
            };

            string route = $"/api/v1/people/{person.Id}";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'passports'.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_deleting_primary_resource_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            Person lockedPerson = _fakers.Person.Generate();
            lockedPerson.IsLocked = true;
            lockedPerson.Passport = new Passport();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(lockedPerson);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/v1/passports/{lockedPerson.Passport.StringId}";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'people'.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_creating_ToMany_relationship_using_BeforeUpdateRelationship_hook()
        {
            // Arrange
            List<Person> persons = _fakers.Person.Generate(2);
            TodoItem lockedTodo = _fakers.TodoItem.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(lockedTodo);
                await dbContext.SaveChangesAsync();
            });

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

            const string route = "/api/v1/todoItems";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'todoItems'.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_replacing_ToMany_relationship_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            List<Person> persons = _fakers.Person.Generate(2);

            TodoItem lockedTodo = _fakers.TodoItem.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();

            TodoItem unlockedTodo = _fakers.TodoItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.AddRange(lockedTodo, unlockedTodo);
                await dbContext.SaveChangesAsync();
            });

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

            string route = $"/api/v1/todoItems/{unlockedTodo.Id}";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'todoItems'.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_clearing_ToMany_relationship_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            List<Person> persons = _fakers.Person.Generate(2);
            TodoItem lockedTodo = _fakers.TodoItem.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(lockedTodo);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/v1/people/{persons[0].Id}";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'todoItems'.");
            error.Detail.Should().BeNull();
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
