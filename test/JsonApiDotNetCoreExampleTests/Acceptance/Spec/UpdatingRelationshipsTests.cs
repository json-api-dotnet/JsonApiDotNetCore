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
    public class UpdatingRelationshipsTests
    {
        private TestFixture<TestStartup> _fixture;
        private AppDbContext _context;
        private Bogus.Faker<Person> _personFaker;
        private Faker<TodoItem> _todoItemFaker;

        public UpdatingRelationshipsTests(TestFixture<TestStartup> fixture)
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
                    type = "todo-items",
                    id = todoItem.Id,
                    relationships = new Dictionary<string, object>
                    {
                        { "children-todos", new
                            {
                                data = new object[]
                                {
                                    new { type = "todo-items", id = $"{todoItem.Id}" },
                                    new { type = "todo-items", id = $"{strayTodoItem.Id}" }
                                }

                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-items/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");


            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            _context = _fixture.GetService<AppDbContext>();

            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                 .Where(ti => ti.Id == todoItem.Id)
                 .Include(ti => ti.ChildrenTodoItems).First();

            updatedTodoItem.ChildrenTodoItems.Any((ti) => ti.Id == todoItem.Id);
            Assert.Contains(updatedTodoItem.ChildrenTodoItems, (ti) => ti.Id == todoItem.Id);
        }

        [Fact]
        public async Task Can_Update_Cyclic_ToOne_Relationship_By_Patching_Resource()
        {
            // Arrange 
            var todoItem = _todoItemFaker.Generate();
            var strayTodoItem = _todoItemFaker.Generate();
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
                    type = "todo-items",
                    id = todoItem.Id,
                    relationships = new Dictionary<string, object>
                    {
                        { "dependent-on-todo", new
                            {
                                data = new { type = "todo-items", id = $"{todoItem.Id}" }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-items/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");


            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            _context = _fixture.GetService<AppDbContext>();


            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                 .Where(ti => ti.Id == todoItem.Id)
                 .Include(ti => ti.DependentTodoItem).First();

            Assert.Equal(todoItem.Id, updatedTodoItem.DependentTodoItemId);
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
                    type = "todo-items",
                    id = todoItem.Id,
                    relationships = new Dictionary<string, object>
                    {
                        { "dependent-on-todo", new
                            {
                                data = new { type = "todo-items", id = $"{todoItem.Id}" }
                            }
                        },
                        { "children-todos", new
                            {
                                data = new object[]
                                {
                                    new { type = "todo-items", id = $"{todoItem.Id}" },
                                    new { type = "todo-items", id = $"{strayTodoItem.Id}" }
                                }
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-items/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            string serializedContent = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(serializedContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");


            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            _context = _fixture.GetService<AppDbContext>();


            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                 .Where(ti => ti.Id == todoItem.Id)
                 .Include(ti => ti.ParentTodoItem).First();

            Assert.Equal(todoItem.Id, updatedTodoItem.ParentTodoItemId);
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_By_Patching_Resource()
        {
            // arrange
            var todoCollection = new TodoItemCollection();
            todoCollection.TodoItems = new List<TodoItem>();
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoCollection.Owner = person;
            todoCollection.TodoItems.Add(todoItem);
            _context.TodoItemCollections.Add(todoCollection);
            _context.SaveChanges();

            var newTodoItem1 = _todoItemFaker.Generate();
            var newTodoItem2 = _todoItemFaker.Generate();
            _context.AddRange(new TodoItem[] { newTodoItem1, newTodoItem2 });
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    type = "todo-collections",
                    id = todoCollection.Id,
                    relationships = new Dictionary<string, object>
                    {
                        { "todo-items", new
                            {
                                data = new object[]
                                {
                                    new { type = "todo-items", id = $"{newTodoItem1.Id}" },
                                    new { type = "todo-items", id = $"{newTodoItem2.Id}" }
                                }

                            }
                        },
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-collections/{todoCollection.Id}";
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
            /// we are expecting two, not three, because the request does 
            /// a "complete replace".
            Assert.Equal(2, updatedTodoItems.Count);
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_By_Patching_Resource_With_Overlap()
        {
            // arrange
            var todoCollection = new TodoItemCollection();
            todoCollection.TodoItems = new List<TodoItem>();
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
                    type = "todo-collections",
                    id = todoCollection.Id,
                    relationships = new Dictionary<string, object>
                    {
                        { "todo-items", new
                            {
                                data = new object[]
                                {
                                    new { type = "todo-items", id = $"{todoItem1.Id}" },
                                    new { type = "todo-items", id = $"{todoItem2.Id}" }
                                }

                            }
                        },
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-collections/{todoCollection.Id}";
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
            // arrange
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
                        type = "todo-items",
                        id = $"{todoItem.Id}"
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/people/{person.Id}/relationships/todo-items";
            var request = new HttpRequestMessage(httpMethod, route);

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
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
            // arrange
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
                data = new
                {
                    type = "person",
                    id = $"{person.Id}"
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-items/{todoItem.Id}/relationships/owner";
            var request = new HttpRequestMessage(httpMethod, route);

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);
            var todoItemsOwner = _context.TodoItems.Include(t => t.Owner).Single(t => t.Id == todoItem.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(todoItemsOwner);
        }

        [Fact]
        public async Task Can_Delete_Relationship_By_Patching_Resource()
        {
            // arrange
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
                    type = "todo-items",
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
            var route = $"/api/v1/todo-items/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
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
        public async Task Can_Delete_Relationship_By_Patching_Relationship()
        {
            // arrange
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
            var route = $"/api/v1/todo-items/{todoItem.Id}/relationships/owner";
            var request = new HttpRequestMessage(httpMethod, route);

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
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
    }
}
