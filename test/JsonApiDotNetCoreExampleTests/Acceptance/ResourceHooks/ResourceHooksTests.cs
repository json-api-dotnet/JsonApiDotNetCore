using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Acceptance.Spec;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public sealed class ResourceDefinitionTests : FunctionalTestCollection<ResourceHooksApplicationFactory>
    {
        private readonly Faker<User> _userFaker;
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;
        private readonly Faker<Article> _articleFaker;
        private readonly Faker<Author> _authorFaker;
        private readonly Faker<Tag> _tagFaker;

        public ResourceDefinitionTests(ResourceHooksApplicationFactory factory) : base(factory)
        {
            _authorFaker = new Faker<Author>()
                .RuleFor(a => a.LastName, f => f.Random.Words(2));

            _articleFaker = new Faker<Article>()
                .RuleFor(a => a.Caption, f => f.Random.AlphaNumeric(10))
                .RuleFor(a => a.Author, f => _authorFaker.Generate());

            _userFaker = new Faker<User>()
                .CustomInstantiator(f => new User(_dbContext))
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.Password, f => f.Internet.Password());

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());

            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());

            _tagFaker = new Faker<Tag>()
                .CustomInstantiator(f => new Tag())
                .RuleFor(a => a.Name, f => f.Random.AlphaNumeric(10));

            var options = (JsonApiOptions) _factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = false;
            options.DisableChildrenPagination = false;
        }

        [Fact]
        public async Task Can_Create_User_With_Password()
        {
            // Arrange
            var user = _userFaker.Generate();

            var serializer = GetSerializer<User>(p => new { p.Password, p.UserName });

            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/users";

            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(serializer.Serialize(user))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // response assertions
            var body = await response.Content.ReadAsStringAsync();
            var returnedUser = _deserializer.DeserializeSingle<User>(body).Data;
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.False(document.SingleData.Attributes.ContainsKey("password"));
            Assert.Equal(user.UserName, document.SingleData.Attributes["userName"]);

            // db assertions
            var dbUser = await _dbContext.Users.FindAsync(returnedUser.Id);
            Assert.Equal(user.UserName, dbUser.UserName);
            Assert.Equal(user.Password, dbUser.Password);
        }

        [Fact]
        public async Task Can_Update_User_Password()
        {
            // Arrange
            var user = _userFaker.Generate();
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            user.Password = _userFaker.Generate().Password;
            var serializer = GetSerializer<User>(p => new { p.Password });
            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/users/{user.Id}";
            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(serializer.Serialize(user))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // response assertions
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.False(document.SingleData.Attributes.ContainsKey("password"));
            Assert.Equal(user.UserName, document.SingleData.Attributes["userName"]);

            // db assertions
            var dbUser = _dbContext.Users.AsNoTracking().Single(u => u.Id == user.Id);
            Assert.Equal(user.Password, dbUser.Password);
        }

        [Fact]
        public async Task Unauthorized_TodoItem()
        {
            // Arrange
            var route = "/api/v1/todoItems/1337";

            // Act
            var response = await _client.GetAsync(route);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to update the author of todo items.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Unauthorized_Passport()
        {
            // Arrange
            var route = "/api/v1/people/1?include=passport";

            // Act
            var response = await _client.GetAsync(route);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to include passports on individual persons.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Unauthorized_Article()
        {
            // Arrange
            var article = _articleFaker.Generate();
            article.Caption = "Classified";
            _dbContext.Articles.Add(article);
            await _dbContext.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}";

            // Act
            var response = await _client.GetAsync(route);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to see this article.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Article_Is_Hidden()
        {
            // Arrange
            var articles = _articleFaker.Generate(3);
            string toBeExcluded = "This should not be included";
            articles[0].Caption = toBeExcluded;

            _dbContext.Articles.AddRange(articles);
            await _dbContext.SaveChangesAsync();

            var route = "/api/v1/articles";

            // Act
            var response = await _client.GetAsync(route);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with body: {body}");
            Assert.DoesNotContain(toBeExcluded, body);
        }

        [Fact]
        public async Task Tag_Is_Hidden()
        {
            // Arrange
            var article = _articleFaker.Generate();
            var tags = _tagFaker.Generate(2);
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
            _dbContext.ArticleTags.AddRange(articleTags);
            await _dbContext.SaveChangesAsync();

            // Workaround for https://github.com/dotnet/efcore/issues/21026
            var options = (JsonApiOptions) _factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = false;
            options.DisableChildrenPagination = true;

            var route = "/api/v1/articles?include=tags";

            // Act
            var response = await _client.GetAsync(route);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with body: {body}");
            Assert.DoesNotContain(toBeExcluded, body);
        }
        ///// <summary>
        ///// In the Cascade Permission Error tests, we ensure that  all the relevant 
        ///// resources are provided in the hook definitions. In this case, 
        ///// re-relating the meta object to a different article would require 
        ///// also a check for the lockedTodo, because we're implicitly updating 
        ///// its foreign key.
        ///// </summary>
        [Fact]
        public async Task Cascade_Permission_Error_Create_ToOne_Relationship()
        {
            // Arrange
            var lockedPerson = _personFaker.Generate();
            lockedPerson.IsLocked = true;
            var passport = new Passport(_dbContext);
            lockedPerson.Passport = passport;
            _dbContext.People.AddRange(lockedPerson);
            await _dbContext.SaveChangesAsync();

            var content = new
            {
                data = new
                {
                    type = "people",
                    relationships = new Dictionary<string, object>
                    {
                        { "passport", new
                            {
                                data = new { type = "passports", id = lockedPerson.Passport.StringId }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/people";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to update fields or relationships of locked todo items.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Cascade_Permission_Error_Updating_ToOne_Relationship()
        {
            // Arrange
            var person = _personFaker.Generate();
            var passport = new Passport(_dbContext) { IsLocked = true };
            person.Passport = passport;
            _dbContext.People.AddRange(person);
            var newPassport = new Passport(_dbContext);
            _dbContext.Passports.Add(newPassport);
            await _dbContext.SaveChangesAsync();

            var content = new
            {
                data = new
                {
                    type = "people",
                    id = person.Id,
                    relationships = new Dictionary<string, object>
                    {
                        { "passport", new
                            {
                                data = new { type = "passports", id = newPassport.StringId }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/people/{person.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to update fields or relationships of locked persons.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Cascade_Permission_Error_Updating_ToOne_Relationship_Deletion()
        {
            // Arrange
            var person = _personFaker.Generate();
            var passport = new Passport(_dbContext) { IsLocked = true };
            person.Passport = passport;
            _dbContext.People.AddRange(person);
            var newPassport = new Passport(_dbContext);
            _dbContext.Passports.Add(newPassport);
            await _dbContext.SaveChangesAsync();

            var content = new
            {
                data = new
                {
                    type = "people",
                    id = person.Id,
                    relationships = new Dictionary<string, object>
                    {
                        { "passport", new
                            {
                                data = (object)null
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/people/{person.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to update fields or relationships of locked persons.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Cascade_Permission_Error_Delete_ToOne_Relationship()
        {
            // Arrange
            var lockedPerson = _personFaker.Generate();
            lockedPerson.IsLocked = true;
            var passport = new Passport(_dbContext);
            lockedPerson.Passport = passport;
            _dbContext.People.AddRange(lockedPerson);
            await _dbContext.SaveChangesAsync();

            var httpMethod = new HttpMethod("DELETE");
            var route = $"/api/v1/passports/{lockedPerson.Passport.StringId}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to update fields or relationships of locked todo items.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Cascade_Permission_Error_Create_ToMany_Relationship()
        {
            // Arrange
            var persons = _personFaker.Generate(2);
            var lockedTodo = _todoItemFaker.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();
            _dbContext.TodoItems.Add(lockedTodo);
            await _dbContext.SaveChangesAsync();

            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    relationships = new Dictionary<string, object>
                    {
                        { "stakeHolders", new
                            {
                                data = new[]
                                {
                                    new { type = "people", id = persons[0].StringId },
                                    new { type = "people", id = persons[1].StringId }
                                }

                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todoItems";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to update fields or relationships of locked todo items.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Cascade_Permission_Error_Updating_ToMany_Relationship()
        {
            // Arrange
            var persons = _personFaker.Generate(2);
            var lockedTodo = _todoItemFaker.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();
            _dbContext.TodoItems.Add(lockedTodo);
            var unlockedTodo = _todoItemFaker.Generate();
            _dbContext.TodoItems.Add(unlockedTodo);
            await _dbContext.SaveChangesAsync();

            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    id = unlockedTodo.Id,
                    relationships = new Dictionary<string, object>
                    {
                        { "stakeHolders", new
                            {
                                data = new[]
                                {
                                    new { type = "people", id = persons[0].StringId },
                                    new { type = "people", id = persons[1].StringId }
                                }

                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{unlockedTodo.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to update fields or relationships of locked todo items.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Cascade_Permission_Error_Delete_ToMany_Relationship()
        {
            // Arrange
            var persons = _personFaker.Generate(2);
            var lockedTodo = _todoItemFaker.Generate();
            lockedTodo.IsLocked = true;
            lockedTodo.StakeHolders = persons.ToHashSet();
            _dbContext.TodoItems.Add(lockedTodo);
            await _dbContext.SaveChangesAsync();

            var httpMethod = new HttpMethod("DELETE");
            var route = $"/api/v1/people/{persons[0].Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("You are not allowed to update fields or relationships of locked todo items.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }
    }
}
