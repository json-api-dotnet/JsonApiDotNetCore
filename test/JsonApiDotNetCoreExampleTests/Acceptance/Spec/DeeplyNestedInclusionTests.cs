using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class DeeplyNestedInclusionTests
    {
        private TestFixture<TestStartup> _fixture;

        public DeeplyNestedInclusionTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
        }

        private void ResetContext(AppDbContext context) 
        {
            context.TodoItems.RemoveRange(context.TodoItems);
            context.TodoItemCollections.RemoveRange(context.TodoItemCollections);
            context.People.RemoveRange(context.People);
            context.PersonRoles.RemoveRange(context.PersonRoles);
        }

        [Fact]
        public async Task Can_Include_Nested_Relationships()
        {
            // arrange
            const string route = "/api/v1/todo-items?include=collection.owner";

            var todoItem = new TodoItem {
                Collection = new TodoItemCollection {
                    Owner = new Person()
                }
            };
        
            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.RemoveRange(context.TodoItems);
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var todoItems = _fixture.DeSerializer.DeserializeList<TodoItem>(body);

            var responseTodoItem = Assert.Single(todoItems);
            Assert.NotNull(responseTodoItem);
            Assert.NotNull(responseTodoItem.Collection);
            Assert.NotNull(responseTodoItem.Collection.Owner);
        }

        [Fact]
        public async Task Can_Include_Nested_HasMany_Relationships()
        {
            // arrange
            const string route = "/api/v1/todo-items?include=collection.todo-items";

            var todoItem = new TodoItem {
                Collection = new TodoItemCollection {
                    Owner = new Person(),
                    TodoItems = new List<TodoItem> {
                        new TodoItem(),
                        new TodoItem()
                    }
                }
            };
        
            
            var context = _fixture.GetService<AppDbContext>();
            ResetContext(context);

            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Documents>(body);
            var included = documents.Included;
            
            Assert.Equal(4, included.Count); // 1 collection, 3 todos
        }

        [Fact]
        public async Task Can_Include_Nested_HasMany_Relationships_BelongsTo()
        {
            // arrange
            const string route = "/api/v1/todo-items?include=collection.todo-items.owner";

            var todoItem = new TodoItem {
                Collection = new TodoItemCollection {
                    Owner = new Person(),
                    TodoItems = new List<TodoItem> {
                        new TodoItem {
                            Owner = new Person()
                        },
                        new TodoItem()
                    }
                }
            };
        
            var context = _fixture.GetService<AppDbContext>();
            ResetContext(context);

            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Documents>(body);
            var included = documents.Included;
            
            Assert.Equal(5, included.Count); // 1 collection, 3 todos, 1 owner
        }

        [Fact]
        public async Task Can_Include_Nested_Relationships_With_Multiple_Paths()
        {
            // arrange
            const string route = "/api/v1/todo-items?include=collection.owner.role,collection.todo-items.owner";

            var todoItem = new TodoItem {
                Collection = new TodoItemCollection {
                    Owner = new Person {
                        Role = new PersonRole()
                    },
                    TodoItems = new List<TodoItem> {
                        new TodoItem {
                            Owner = new Person()
                        },
                        new TodoItem()
                    }
                }
            };
        
            var context = _fixture.GetService<AppDbContext>();
            ResetContext(context);

            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Documents>(body);
            var included = documents.Included;
            
            Assert.Equal(7, included.Count); // 1 collection, 3 todos, 2 owners, 1 role
        }

        [Fact]
        public async Task Included_Resources_Are_Correct()
        {
            // arrange
            var role = new PersonRole();
            var asignee = new Person { Role = role };
            var collectionOwner = new Person();
            var someOtherOwner = new Person();
            var collection = new TodoItemCollection { Owner = collectionOwner };
            var todoItem1 = new TodoItem { Collection = collection, Assignee = asignee };
            var todoItem2 = new TodoItem { Collection = collection, Assignee = asignee };
            var todoItem3 = new TodoItem { Collection = collection, Owner = someOtherOwner };
            var todoItem4 = new TodoItem { Collection = collection, Owner = asignee };


            string route = 
                "/api/v1/todo-items/" + todoItem1.Id + "?include=" +
                    "collection.owner," + 
                    "asignee.role," + 
                    "asignee.assigned-todo-items";

        
            var context = _fixture.GetService<AppDbContext>();
            ResetContext(context);

            context.TodoItems.Add(todoItem1);
            context.TodoItems.Add(todoItem2);
            context.TodoItems.Add(todoItem3);
            context.TodoItems.Add(todoItem4);
            context.PersonRoles.Add(role);
            context.People.Add(asignee);
            context.People.Add(collectionOwner);
            context.People.Add(someOtherOwner);
            context.TodoItemCollections.Add(collection);


            await context.SaveChangesAsync();

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(body);
            var included = documents.Included;
            
            // 1 collection, 1 owner, 
            // 1 asignee, 1 asignee role,
            // 2 assigned todo items
            Assert.Equal(6, included.Count); 
        }        
    }
}