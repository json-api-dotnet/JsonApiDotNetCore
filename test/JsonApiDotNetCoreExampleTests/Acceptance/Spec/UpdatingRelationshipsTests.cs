using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class UpdatingRelationshipsTests
    {
        private readonly TestFixture<Startup> _fixture;
        private AppDbContext _context;
        private readonly Faker<Person> _personFaker;
        private readonly Faker<TodoItem> _todoItemFaker;

        public UpdatingRelationshipsTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
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
            _context.SaveChanges();


            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

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
                        { "childrenTodos", new
                            {
                                data = new object[]
                                {
                                    new { type = "todoItems", id = $"{todoItem.Id}" },
                                    new { type = "todoItems", id = $"{strayTodoItem.Id}" }
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");


            // Act
            await client.SendAsync(request);
            _context = _fixture.GetService<AppDbContext>();

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
            _context.SaveChanges();


            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

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
                        { "dependentOnTodo", new
                            {
                                data = new { type = "todoItems", id = $"{todoItem.Id}" }
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");


            // Act
            await client.SendAsync(request);
            _context = _fixture.GetService<AppDbContext>();

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
            _context.SaveChanges();


            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

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
                        { "dependentOnTodo", new
                            {
                                data = new { type = "todoItems", id = $"{todoItem.Id}" }
                            }
                        },
                        { "childrenTodos", new
                            {
                                data = new object[]
                                {
                                    new { type = "todoItems", id = $"{todoItem.Id}" },
                                    new { type = "todoItems", id = $"{strayTodoItem.Id}" }
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");


            // Act
            await client.SendAsync(request);
            _context = _fixture.GetService<AppDbContext>();

            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                 .Where(ti => ti.Id == todoItem.Id)
                 .Include(ti => ti.ParentTodo).First();

            Assert.Equal(todoItem.Id, updatedTodoItem.ParentTodoId);
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_By_Patching_Resource()
        {
            // Arrange
            var todoCollection = new TodoItemCollection {TodoItems = new List<TodoItem>()};
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoCollection.Owner = person;
            todoCollection.TodoItems.Add(todoItem);
            _context.TodoItemCollections.Add(todoCollection);
            _context.SaveChanges();

            var newTodoItem1 = _todoItemFaker.Generate();
            var newTodoItem2 = _todoItemFaker.Generate();
            _context.AddRange(newTodoItem1, newTodoItem2);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

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
                        { "todoItems", new
                            {
                                data = new object[]
                                {
                                    new { type = "todoItems", id = $"{newTodoItem1.Id}" },
                                    new { type = "todoItems", id = $"{newTodoItem2.Id}" }
                                }

                            }
                        },
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoCollections/{todoCollection.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);
            _context = _fixture.GetService<AppDbContext>();
            var updatedTodoItems = _context.TodoItemCollections.AsNoTracking()
                .Where(tic => tic.Id == todoCollection.Id)
                .Include(tdc => tdc.TodoItems).SingleOrDefault().TodoItems;


            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // we are expecting two, not three, because the request does 
            // a "complete replace".
            Assert.Equal(2, updatedTodoItems.Count);
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_By_Patching_Resource_When_Targets_Already_Attached()
        {
            // It is possible that entities we're creating relationships to
            // have already been included in dbContext the application beyond control
            // of JANDC. For example: a user may have been loaded when checking permissions
            // in business logic in controllers. In this case,
            // this user may not be reattached to the db context in the repository.

            // Arrange
            var todoCollection = new TodoItemCollection {TodoItems = new List<TodoItem>()};
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoCollection.Owner = person;
            todoCollection.Name = "PRE-ATTACH-TEST";
            todoCollection.TodoItems.Add(todoItem);
            _context.TodoItemCollections.Add(todoCollection);
            _context.SaveChanges();

            var newTodoItem1 = _todoItemFaker.Generate();
            var newTodoItem2 = _todoItemFaker.Generate();
            _context.AddRange(newTodoItem1, newTodoItem2);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

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
                        { "todoItems", new
                            {
                                data = new object[]
                                {
                                    new { type = "todoItems", id = $"{newTodoItem1.Id}" },
                                    new { type = "todoItems", id = $"{newTodoItem2.Id}" }
                                }

                            }
                        },
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoCollections/{todoCollection.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);
            _context = _fixture.GetService<AppDbContext>();
            var updatedTodoItems = _context.TodoItemCollections.AsNoTracking()
                .Where(tic => tic.Id == todoCollection.Id)
                .Include(tdc => tdc.TodoItems).SingleOrDefault().TodoItems;


            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // we are expecting two, not three, because the request does 
            // a "complete replace".
            Assert.Equal(2, updatedTodoItems.Count);
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_By_Patching_Resource_With_Overlap()
        {
            // Arrange
            var todoCollection = new TodoItemCollection {TodoItems = new List<TodoItem>()};
            var person = _personFaker.Generate();
            var todoItem1 = _todoItemFaker.Generate();
            var todoItem2 = _todoItemFaker.Generate();
            todoCollection.Owner = person;
            todoCollection.TodoItems.Add(todoItem1);
            todoCollection.TodoItems.Add(todoItem2);
            _context.TodoItemCollections.Add(todoCollection);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

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
                        { "todoItems", new
                            {
                                data = new object[]
                                {
                                    new { type = "todoItems", id = $"{todoItem1.Id}" },
                                    new { type = "todoItems", id = $"{todoItem2.Id}" }
                                }

                            }
                        },
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoCollections/{todoCollection.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);


            _context = _fixture.GetService<AppDbContext>();
            var updatedTodoItems = _context.TodoItemCollections.AsNoTracking()
                .Where(tic => tic.Id == todoCollection.Id)
                .Include(tdc => tdc.TodoItems).SingleOrDefault().TodoItems;


            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, updatedTodoItems.Count);
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_ThroughLink()
        {
            // Arrange
            var person = _personFaker.Generate();
            _context.People.Add(person);

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);

            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new List<object>
                {
                    new {
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

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);
            _context = _fixture.GetService<AppDbContext>();
            var personsTodoItems = _context.People.Include(p => p.TodoItems).Single(p => p.Id == person.Id).TodoItems;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(personsTodoItems);
        }

        [Fact]
        public async Task Can_Update_ToOne_Relationship_ThroughLink()
        {
            // Arrange
            var person = _personFaker.Generate();
            _context.People.Add(person);

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);

            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var serializer = _fixture.GetSerializer<Person>(p => new { });
            var content = serializer.Serialize(person);

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}/relationships/owner";
            var request = new HttpRequestMessage(httpMethod, route) {Content = new StringContent(content)};

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);
            var todoItemsOwner = _context.TodoItems.Include(t => t.Owner).Single(t => t.Id == todoItem.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(todoItemsOwner);
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
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

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
                            data = (object)null
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var todoItemResult = _context.TodoItems
                .AsNoTracking()
                .Include(t => t.Owner)
                .Single(t => t.Id == todoItem.Id);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(todoItemResult.Owner);
        }


        [Fact]
        public async Task Can_Delete_ToMany_Relationship_By_Patching_Resource()
        {
            // Arrange
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            person.TodoItems = new List<TodoItem> { todoItem };
            _context.People.Add(person);
            _context.SaveChanges();

            var content = new
            {
                data = new
                {
                    id = person.Id,
                    type = "people",
                    relationships = new Dictionary<string, object>
                    {
                         { "todoItems", new
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            var personResult = _context.People
                .AsNoTracking()
                .Include(p => p.TodoItems)
                .Single(p => p.Id == person.Id);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(personResult.TodoItems);
        }

        [Fact]
        public async Task Can_Delete_Relationship_By_Patching_Relationship()
        {
            // Arrange
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;

            _context.People.Add(person);
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = (object)null
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}/relationships/owner";
            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var todoItemResult = _context.TodoItems
                .AsNoTracking()
                .Include(t => t.Owner)
                .Single(t => t.Id == todoItem.Id);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(todoItemResult.Owner);
        }

        [Fact]
        public async Task Updating_ToOne_Relationship_With_Implicit_Remove()
        {
            // Arrange
            var context = _fixture.GetService<AppDbContext>();
            var passport = new Passport();
            var person1 = _personFaker.Generate();
            person1.Passport = passport;
            var person2 = _personFaker.Generate();
            context.People.AddRange(new List<Person> { person1, person2 });
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
                        { "passport", new
                            {
                                data = new { type = "passports", id = $"{passportId}" }
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
           
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");
            var dbPerson = context.People.AsNoTracking().Where(p => p.Id == person2.Id).Include("Passport").FirstOrDefault();
            Assert.Equal(passportId, dbPerson.Passport.Id);
        }

        [Fact]
        public async Task Updating_ToMany_Relationship_With_Implicit_Remove()
        {
            // Arrange
            var context = _fixture.GetService<AppDbContext>();
            var person1 = _personFaker.Generate();
            person1.TodoItems = _todoItemFaker.Generate(3).ToList();
            var person2 = _personFaker.Generate();
            person2.TodoItems = _todoItemFaker.Generate(2).ToList();
            context.People.AddRange(new List<Person> { person1, person2 });
            await context.SaveChangesAsync();
            var todoItem1Id = person1.TodoItems[0].Id;
            var todoItem2Id = person1.TodoItems[1].Id;

            var content = new
            {
                data = new
                {
                    type = "people",
                    id = person2.Id,
                    relationships = new Dictionary<string, object>
                    {
                        { "todoItems", new
                            {
                                data = new List<object>
                                {
                                    new {
                                        type = "todoItems",
                                        id = $"{todoItem1Id}"
                                    },
                                    new {
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert

            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");
            var dbPerson = context.People.AsNoTracking().Where(p => p.Id == person2.Id).Include("TodoItems").FirstOrDefault();
            Assert.Equal(2, dbPerson.TodoItems.Count);
            Assert.NotNull(dbPerson.TodoItems.SingleOrDefault(ti => ti.Id == todoItem1Id));
            Assert.NotNull(dbPerson.TodoItems.SingleOrDefault(ti => ti.Id == todoItem2Id));
        }
    }
}
