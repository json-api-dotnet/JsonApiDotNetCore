using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Client.Internal;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Definitions;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
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

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = false;
            options.DisableChildrenPagination = false;
        }

        [Fact]
        public async Task Can_create_user_with_password()
        {
            // Arrange
            var user = _fakers.User.Generate();

            var serializer = GetRequestSerializer<User>(p => new {p.Password, p.UserName});
            string requestBody = serializer.Serialize(user);

            var route = "/api/v1/users";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            var responseUser = GetResponseDeserializer().DeserializeSingle<User>(responseDocument).Data;
            var document = JsonConvert.DeserializeObject<Document>(responseDocument);

            document.SingleData.Attributes.Should().NotContainKey("password");
            document.SingleData.Attributes["userName"].Should().Be(user.UserName);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var userInDatabase = await dbContext.Users.FirstAsync(u => u.Id == responseUser.Id);

                userInDatabase.UserName.Should().Be(user.UserName);
                userInDatabase.Password.Should().Be(user.Password);
            });
        }

        [Fact]
        public async Task Can_update_user_password()
        {
            // Arrange
            var user = _fakers.User.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            });

            user.Password = _fakers.User.Generate().Password;

            var serializer = GetRequestSerializer<User>(p => new {p.Password});
            string requestBody = serializer.Serialize(user);

            var route = $"/api/v1/users/{user.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Attributes.Should().NotContainKey("password");
            responseDocument.SingleData.Attributes["userName"].Should().Be(user.UserName);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var userInDatabase = await dbContext.Users.FirstAsync(u => u.Id == user.Id);

                userInDatabase.Password.Should().Be(user.Password);
            });
        }

        [Fact]
        public async Task Can_block_access_to_resource_from_GetSingle_endpoint_using_BeforeRead_hook()
        {
            // Arrange
            var route = "/api/v1/todoItems/1337";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to update the author of todo items.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_access_to_included_resource_using_BeforeRead_hook()
        {
            // Arrange
            var route = "/api/v1/people/1?include=passport";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to include passports on individual persons.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_access_to_resource_from_GetSingle_endpoint_using_OnReturn_hook()
        {
            // Arrange
            var article = _fakers.Article.Generate();
            article.Caption = "Classified";

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to see this article.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_hide_primary_resource_from_result_set_from_GetAll_endpoint_using_OnReturn_hook()
        {
            // Arrange
            var articles = _fakers.Article.Generate(3);
            string toBeExcluded = "This should not be included";
            articles[0].Caption = toBeExcluded;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.AddRange(articles);
                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Can_hide_secondary_resource_from_ToOne_relationship_using_OnReturn_hook()
        {
            // Arrange
            var person = _fakers.Person.Generate();
            person.Passport = new Passport {IsLocked = true};

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(person);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/people/{person.Id}/passport";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }


        [Fact]
        public async Task Can_hide_secondary_resource_from_ToMany_List_relationship_using_OnReturn_hook()
        {
            // Arrange
            string toBeExcluded = "This should not be included";

            var author = _fakers.Author.Generate();
            author.Articles = _fakers.Article.Generate(3);
            author.Articles[0].Caption = toBeExcluded;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AuthorDifferentDbContextName.Add(author);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/authors/{author.Id}/articles";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Can_hide_secondary_resource_from_ToMany_Set_relationship_using_OnReturn_hook()
        {
            // Arrange
            string toBeExcluded = "This should not be included";

            var person = _fakers.Person.Generate();
            person.TodoItems = _fakers.TodoItem.Generate(3).ToHashSet();
            person.TodoItems.First().Description = toBeExcluded;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(person);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/people/{person.Id}/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Can_hide_resource_from_included_HasManyThrough_relationship_using_OnReturn_hook()
        {
            // Arrange
            string toBeExcluded = "This should not be included";

            var tags = _fakers.Tag.Generate(2);
            tags[0].Name = toBeExcluded;

            var article = _fakers.Article.Generate();
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
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = false;
            options.DisableChildrenPagination = true;

            var route = "/api/v1/articles?include=tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().NotContain(toBeExcluded);
        }

        [Fact]
        public async Task Can_block_creating_ToOne_relationship_using_BeforeUpdateRelationship_hook()
        {
            // Arrange
            var lockedPerson = _fakers.Person.Generate();
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

            var route = "/api/v1/people";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'people'.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_replacing_ToOne_relationship_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            var person = _fakers.Person.Generate();
            person.Passport = new Passport {IsLocked = true};

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

            var route = $"/api/v1/people/{person.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'passports'.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_clearing_ToOne_relationship_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            var person = _fakers.Person.Generate();
            person.Passport = new Passport {IsLocked = true};

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
                            data = (object) null
                        }
                    }
                }
            };

            var route = $"/api/v1/people/{person.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'passports'.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_deleting_primary_resource_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            var lockedPerson = _fakers.Person.Generate();
            lockedPerson.IsLocked = true;
            lockedPerson.Passport = new Passport();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(lockedPerson);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/passports/{lockedPerson.Passport.StringId}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'people'.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_creating_ToMany_relationship_using_BeforeUpdateRelationship_hook()
        {
            // Arrange
            var persons = _fakers.Person.Generate(2);
            var lockedTodo = _fakers.TodoItem.Generate();
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

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'todoItems'.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_replacing_ToMany_relationship_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            var persons = _fakers.Person.Generate(2);

            var lockedTodo = _fakers.TodoItem.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();

            var unlockedTodo = _fakers.TodoItem.Generate();

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

            var route = $"/api/v1/todoItems/{unlockedTodo.Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'todoItems'.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_block_clearing_ToMany_relationship_using_BeforeImplicitUpdateRelationship_hook()
        {
            // Arrange
            var persons = _fakers.Person.Generate(2);
            var lockedTodo = _fakers.TodoItem.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(lockedTodo);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/people/{persons[0].Id}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("You are not allowed to update fields or relationships of locked resource of type 'todoItems'.");
            responseDocument.Errors[0].Detail.Should().BeNull();
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
