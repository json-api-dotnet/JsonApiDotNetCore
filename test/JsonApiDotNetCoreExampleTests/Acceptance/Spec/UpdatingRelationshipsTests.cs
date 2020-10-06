using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class UpdatingRelationshipsTests
    {
        private readonly TestFixture<TestStartup> _fixture;
        private AppDbContext _context;
        private readonly Faker<Person> _personFaker;
        private readonly Faker<TodoItem> _todoItemFaker;

        public UpdatingRelationshipsTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetRequiredService<AppDbContext>();
            _personFaker = new Faker<Person>()
                .RuleFor(t => t.FirstName, f => f.Name.FirstName())
                .RuleFor(t => t.LastName, f => f.Name.LastName());

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Fact]
        public async Task Can_Update_Cyclic_ToMany_Relationship_By_Patching_Resource()
        {
            // Arrange 
            var todoItem = _todoItemFaker.Generate();
            var strayTodoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            _context.TodoItems.Add(strayTodoItem);
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Act
            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.Id,
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "childrenTodos", new
                            {
                                data = new object[]
                                {
                                    new {type = "todoItems", id = $"{todoItem.Id}"},
                                    new {type = "todoItems", id = $"{strayTodoItem.Id}"}
                                }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);


            // Act
            await client.SendAsync(request);
            _context = _fixture.GetRequiredService<AppDbContext>();

            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                .Where(ti => ti.Id == todoItem.Id)
                .Include(ti => ti.ChildrenTodos).First();

            Assert.Contains(updatedTodoItem.ChildrenTodos, ti => ti.Id == todoItem.Id);
        }

        [Fact]
        public async Task Can_Update_Cyclic_ToOne_Relationship_By_Patching_Resource()
        {
            // Arrange 
            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Act
            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.Id,
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "dependentOnTodo", new
                            {
                                data = new {type = "todoItems", id = $"{todoItem.Id}"}
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);


            // Act
            await client.SendAsync(request);
            _context = _fixture.GetRequiredService<AppDbContext>();

            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                .Where(ti => ti.Id == todoItem.Id)
                .Include(ti => ti.DependentOnTodo).First();

            Assert.Equal(todoItem.Id, updatedTodoItem.DependentOnTodoId);
        }

        [Fact]
        public async Task Can_Update_Both_Cyclic_ToOne_And_ToMany_Relationship_By_Patching_Resource()
        {
            // Arrange 
            var todoItem = _todoItemFaker.Generate();
            var strayTodoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            _context.TodoItems.Add(strayTodoItem);
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Act
            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.Id,
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "dependentOnTodo", new
                            {
                                data = new {type = "todoItems", id = $"{todoItem.Id}"}
                            }
                        },
                        {
                            "childrenTodos", new
                            {
                                data = new object[]
                                {
                                    new {type = "todoItems", id = $"{todoItem.Id}"},
                                    new {type = "todoItems", id = $"{strayTodoItem.Id}"}
                                }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);


            // Act
            await client.SendAsync(request);
            _context = _fixture.GetRequiredService<AppDbContext>();

            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                .Where(ti => ti.Id == todoItem.Id)
                .Include(ti => ti.ParentTodo).First();

            Assert.Equal(todoItem.Id, updatedTodoItem.ParentTodoId);
        }

        [Fact]
        public async Task Fails_When_Patching_Resource_ToOne_Relationship_With_Missing_Resource()
        {
            // Arrange 
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            _context.AddRange(todoItem, person);
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Act
            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.Id,
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "stakeHolders", new
                            {
                                data = new[]
                                {
                                    new { type = "people", id = person.StringId },
                                    new { type = "people", id = "900000" },
                                    new { type = "people", id = "900001" }
                                }
                            }
                        },
                        {
                            "parentTodo", new
                            {
                                data = new { type = "todoItems", id = "900002" }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.NotFound, response);
            Assert.Contains("For the following types, the resources with the specified ids do not exist:\\\\npeople: 900000,900001\\ntodoItems: 900002\"", responseBody);
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_By_Patching_Resource()
        {
            // Arrange
            var todoCollection = new TodoItemCollection {TodoItems = new HashSet<TodoItem>()};
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoCollection.Owner = person;
            todoCollection.TodoItems.Add(todoItem);
            _context.TodoItemCollections.Add(todoCollection);
            await _context.SaveChangesAsync();

            var newTodoItem1 = _todoItemFaker.Generate();
            var newTodoItem2 = _todoItemFaker.Generate();
            _context.AddRange(newTodoItem1, newTodoItem2);
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    type = "todoCollections",
                    id = todoCollection.Id,
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "todoItems", new
                            {
                                data = new object[]
                                {
                                    new {type = "todoItems", id = $"{newTodoItem1.Id}"},
                                    new {type = "todoItems", id = $"{newTodoItem2.Id}"}
                                }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoCollections/{todoCollection.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);
            _context = _fixture.GetRequiredService<AppDbContext>();
            var updatedTodoItems = _context.TodoItemCollections.AsNoTracking()
                .Where(tic => tic.Id == todoCollection.Id)
                .Include(tdc => tdc.TodoItems).SingleOrDefault().TodoItems;


            AssertEqualStatusCode(HttpStatusCode.OK, response);
            // we are expecting two, not three, because the request does 
            // a "complete replace".
            Assert.Equal(2, updatedTodoItems.Count);
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_By_Patching_Resource_When_Targets_Already_Attached()
        {
            // It is possible that resources we're creating relationships to
            // have already been included in dbContext the application beyond control
            // of JANDC. For example: a user may have been loaded when checking permissions
            // in business logic in controllers. In this case,
            // this user may not be reattached to the db context in the repository.

            // Arrange
            var todoCollection = new TodoItemCollection {TodoItems = new HashSet<TodoItem>()};
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoCollection.Owner = person;
            todoCollection.Name = "PRE-ATTACH-TEST";
            todoCollection.TodoItems.Add(todoItem);
            _context.TodoItemCollections.Add(todoCollection);
            await _context.SaveChangesAsync();

            var newTodoItem1 = _todoItemFaker.Generate();
            var newTodoItem2 = _todoItemFaker.Generate();
            _context.AddRange(newTodoItem1, newTodoItem2);
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    type = "todoCollections",
                    id = todoCollection.Id,
                    attributes = new
                    {
                        name = todoCollection.Name
                    },
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "todoItems", new
                            {
                                data = new object[]
                                {
                                    new {type = "todoItems", id = $"{newTodoItem1.Id}"},
                                    new {type = "todoItems", id = $"{newTodoItem2.Id}"}
                                }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoCollections/{todoCollection.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);
            _context = _fixture.GetRequiredService<AppDbContext>();
            var updatedTodoItems = _context.TodoItemCollections.AsNoTracking()
                .Where(tic => tic.Id == todoCollection.Id)
                .Include(tdc => tdc.TodoItems).SingleOrDefault().TodoItems;


            AssertEqualStatusCode(HttpStatusCode.OK, response);
            // we are expecting two, not three, because the request does 
            // a "complete replace".
            Assert.Equal(2, updatedTodoItems.Count);
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_By_Patching_Resource_With_Overlap()
        {
            // Arrange
            var todoCollection = new TodoItemCollection { TodoItems = new HashSet<TodoItem>() };
            var person = _personFaker.Generate();
            var todoItem1 = _todoItemFaker.Generate();
            var todoItem2 = _todoItemFaker.Generate();
            todoCollection.Owner = person;
            todoCollection.TodoItems.Add(todoItem1);
            todoCollection.TodoItems.Add(todoItem2);
            _context.TodoItemCollections.Add(todoCollection);
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    type = "todoCollections",
                    id = todoCollection.Id,
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "todoItems", new
                            {
                                data = new object[]
                                {
                                    new {type = "todoItems", id = $"{todoItem1.Id}"},
                                    new {type = "todoItems", id = $"{todoItem2.Id}"}
                                }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoCollections/{todoCollection.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);


            _context = _fixture.GetRequiredService<AppDbContext>();
            var updatedTodoItems = _context.TodoItemCollections.AsNoTracking()
                .Where(tic => tic.Id == todoCollection.Id)
                .Include(tdc => tdc.TodoItems).SingleOrDefault().TodoItems;


            AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Equal(2, updatedTodoItems.Count);
        }

        [Fact]
        public async Task Can_Delete_ToOne_Relationship_By_Patching_Resource()
        {
            // Arrange
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;

            _context.People.Add(person);
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    id = todoItem.Id,
                    type = "todoItems",
                    relationships = new
                    {
                        owner = new
                        {
                            data = (object) null
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var todoItemResult = _context.TodoItems
                .AsNoTracking()
                .Include(t => t.Owner)
                .Single(t => t.Id == todoItem.Id);

            AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Null(todoItemResult.Owner);
        }

        [Fact]
        public async Task Can_Delete_ToMany_Relationship_By_Patching_Resource()
        {
            // Arrange
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            person.TodoItems = new HashSet<TodoItem> {todoItem};
            _context.People.Add(person);
            await _context.SaveChangesAsync();

            var content = new
            {
                data = new
                {
                    id = person.Id,
                    type = "people",
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "todoItems", new
                            {
                                data = new List<object>()
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
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            var personResult = _context.People
                .AsNoTracking()
                .Include(p => p.TodoItems)
                .Single(p => p.Id == person.Id);

            AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Empty(personResult.TodoItems);
        }

        [Fact]
        public async Task Updating_ToOne_Relationship_With_Implicit_Remove()
        {
            // Arrange
            var context = _fixture.GetRequiredService<AppDbContext>();
            var passport = new Passport(context);
            var person1 = _personFaker.Generate();
            person1.Passport = passport;
            var person2 = _personFaker.Generate();
            context.People.AddRange(new List<Person> {person1, person2});
            await context.SaveChangesAsync();
            var passportId = person1.PassportId;
            var content = new
            {
                data = new
                {
                    type = "people",
                    id = person2.Id,
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "passport", new
                            {
                                data = new {type = "passports", id = $"{passport.StringId}"}
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/people/{person2.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert

            Assert.True(HttpStatusCode.OK == response.StatusCode,
                $"{route} returned {response.StatusCode} status code with payload: {body}");
            var dbPerson = context.People.AsNoTracking().Where(p => p.Id == person2.Id).Include("Passport")
                .FirstOrDefault();
            Assert.Equal(passportId, dbPerson.Passport.Id);
        }

        [Fact]
        public async Task Updating_ToMany_Relationship_With_Implicit_Remove()
        {
            // Arrange
            var context = _fixture.GetRequiredService<AppDbContext>();
            var person1 = _personFaker.Generate();
            person1.TodoItems = _todoItemFaker.Generate(3).ToHashSet();
            var person2 = _personFaker.Generate();
            person2.TodoItems = _todoItemFaker.Generate(2).ToHashSet();
            context.People.AddRange(new List<Person> {person1, person2});
            await context.SaveChangesAsync();
            var todoItem1Id = person1.TodoItems.ElementAt(0).Id;
            var todoItem2Id = person1.TodoItems.ElementAt(1).Id;

            var content = new
            {
                data = new
                {
                    type = "people",
                    id = person2.Id,
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "todoItems", new
                            {
                                data = new List<object>
                                {
                                    new
                                    {
                                        type = "todoItems",
                                        id = $"{todoItem1Id}"
                                    },
                                    new
                                    {
                                        type = "todoItems",
                                        id = $"{todoItem2Id}"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/people/{person2.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert

            Assert.True(HttpStatusCode.OK == response.StatusCode,
                $"{route} returned {response.StatusCode} status code with payload: {body}");
            var dbPerson = context.People.AsNoTracking().Where(p => p.Id == person2.Id).Include("TodoItems")
                .FirstOrDefault();
            Assert.Equal(2, dbPerson.TodoItems.Count);
            Assert.NotNull(dbPerson.TodoItems.SingleOrDefault(ti => ti.Id == todoItem1Id));
            Assert.NotNull(dbPerson.TodoItems.SingleOrDefault(ti => ti.Id == todoItem2Id));
        }

        [Fact]
        public async Task Can_Set_ToMany_Relationship_Through_Relationship_Endpoint()
        {
            // Arrange
            var person = _personFaker.Generate();
            person.TodoItems = _todoItemFaker.Generate(3).ToHashSet();
            _context.People.Add(person);

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);

            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new List<object>
                {
                    new
                    {
                        type = "todoItems",
                        id = $"{todoItem.Id}"
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/people/{person.Id}/relationships/todoItems";
            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request); ;
            var responseBody = await response.Content.ReadAsStringAsync();
            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            _context = _fixture.GetRequiredService<AppDbContext>();
            var assertTodoItems = _context.People.Include(p => p.TodoItems)
                .Single(p => p.Id == person.Id).TodoItems;

            Assert.Single(assertTodoItems);
            Assert.Equal(todoItem.Id, assertTodoItems.ElementAt(0).Id);
        }

        [Fact]
        public async Task Can_Set_ToOne_Relationship_Through_Relationship_Endpoint()
        {
            // Arrange
            var person = _personFaker.Generate();
            _context.People.Add(person);

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);

            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var serializer = _fixture.GetSerializer<Person>(p => new { });
            var content = serializer.Serialize(person);

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}/relationships/owner";
            var request = new HttpRequestMessage(httpMethod, route) {Content = new StringContent(content)};

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);
            var todoItemsOwner = _context.TodoItems.Include(t => t.Owner).Single(t => t.Id == todoItem.Id);

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.NotNull(todoItemsOwner);
        }

        [Fact]
        public async Task Can_Delete_Relationship_By_Patching_Through_Relationship_Endpoint()
        {
            // Arrange
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;

            _context.People.Add(person);
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = (object) null
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}/relationships/owner";
            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            // Assert
            var todoItemResult = _context.TodoItems
                .AsNoTracking()
                .Include(t => t.Owner)
                .Single(t => t.Id == todoItem.Id);

            AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Null(todoItemResult.Owner);
        }

        [Fact]
        public async Task Can_Add_To_ToMany_Relationship_Through_Relationship_Endpoint()
        {
            // Arrange
            var person = _personFaker.Generate();
            person.TodoItems = _todoItemFaker.Generate(3).ToHashSet();
            _context.People.Add(person);

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);

            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new List<object>
                {
                    new
                    {
                        type = "todoItems",
                        id = $"{todoItem.Id}"
                    }
                }
            };

            var httpMethod = new HttpMethod("POST");
            var route = $"/api/v1/people/{person.Id}/relationships/todoItems";
            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            _context = _fixture.GetRequiredService<AppDbContext>();
            var assertTodoItems = _context.People.Include(p => p.TodoItems)
                .Single(p => p.Id == person.Id).TodoItems;

            Assert.Equal(4, assertTodoItems.Count);
            Assert.True(assertTodoItems.Any(ati => ati.Id == todoItem.Id));
        }

        [Fact]
        public async Task Can_Delete_From_To_ToMany_Relationship_Through_Relationship_Endpoint()
        {
            // Arrange
            var person = _personFaker.Generate();
            person.TodoItems = _todoItemFaker.Generate(3).ToHashSet();
            _context.People.Add(person);

            await _context.SaveChangesAsync();
            var todoItemToDelete = person.TodoItems.ElementAt(0);

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new List<object>
                {
                    new
                    {
                        type = "todoItems",
                        id = $"{todoItemToDelete.Id}"
                    }
                }
            };

            var httpMethod = new HttpMethod("DELETE");
            var route = $"/api/v1/people/{person.Id}/relationships/todoItems";
            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            _context = _fixture.GetRequiredService<AppDbContext>();
            var assertTodoItems = _context.People.AsNoTracking().Include(p => p.TodoItems)
                .Single(p => p.Id == person.Id).TodoItems;

            Assert.Equal(2, assertTodoItems.Count);
            var deletedTodoItem = assertTodoItems.SingleOrDefault(ti => ti.Id == todoItemToDelete.Id);
            Assert.Null(deletedTodoItem);
        }

        [Fact]
        public async Task Fails_When_Unknown_Relationship_On_Relationship_Endpoint()
        {
            // Arrange
            var person = _personFaker.Generate();
            _context.People.Add(person);

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);

            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var serializer = _fixture.GetSerializer<Person>(p => new { });
            var content = serializer.Serialize(person);

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}/relationships/invalid";
            var request = new HttpRequestMessage(httpMethod, route) {Content = new StringContent(content)};

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.NotFound, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal("The requested relationship does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("The resource 'todoItems' does not contain a relationship named 'invalid'.",
                errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Fails_When_Missing_Resource_On_Relationship_Endpoint()
        {
            // Arrange
            var person = _personFaker.Generate();
            _context.People.Add(person);

            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var serializer = _fixture.GetSerializer<Person>(p => new { });
            var content = serializer.Serialize(person);

            var httpMethod = new HttpMethod("PATCH");
            var route = "/api/v1/todoItems/99999999/relationships/owner";
            var request = new HttpRequestMessage(httpMethod, route) {Content = new StringContent(content)};

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            AssertEqualStatusCode(HttpStatusCode.NotFound, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal("The requested resource does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("Resource of type 'todoItems' with ID '99999999' does not exist.",
                errorDocument.Errors[0].Detail);
        }
        
        private void AssertEqualStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            Assert.True(expected == response.StatusCode, $"Got {response.StatusCode} status code with payload instead of {expected}. Payload: {response.Content.ReadAsStringAsync().Result}");
        }
    }
}
