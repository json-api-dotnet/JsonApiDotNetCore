using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
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

        //[Fact]
        //public async Task Can_Include_Nested_Relationships()
        //{
        //    // arrange
        //    const string route = "/api/v1/todo-items?include=collection.owner";

        //    var todoItem = new TodoItem {
        //        Collection = new TodoItemCollection {
        //            Owner = new Person()
        //        }
        //    };
        
        //    var context = _fixture.GetService<AppDbContext>();
        //    context.TodoItems.RemoveRange(context.TodoItems);
        //    context.TodoItems.Add(todoItem);
        //    await context.SaveChangesAsync();

        //    // act
        //    var response = await _fixture.Client.GetAsync(route);

        //    // assert
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    var body = await response.Content.ReadAsStringAsync();
        //    var todoItems = _fixture.deserializer.DeserializeList<TodoItem>(body);

        //    var responseTodoItem = Assert.Single(todoItems);
        //    Assert.NotNull(responseTodoItem);
        //    Assert.NotNull(responseTodoItem.Collection);
        //    Assert.NotNull(responseTodoItem.Collection.Owner);
        //}

        //[Fact]
        //public async Task Can_Include_Nested_HasMany_Relationships()
        //{
        //    // arrange
        //    const string route = "/api/v1/todo-items?include=collection.todo-items";

        //    var todoItem = new TodoItem {
        //        Collection = new TodoItemCollection {
        //            Owner = new Person(),
        //            TodoItems = new List<TodoItem> {
        //                new TodoItem(),
        //                new TodoItem()
        //            }
        //        }
        //    };
        
            
        //    var context = _fixture.GetService<AppDbContext>();
        //    ResetContext(context);

        //    context.TodoItems.Add(todoItem);
        //    await context.SaveChangesAsync();

        //    // act
        //    var response = await _fixture.Client.GetAsync(route);

        //    // assert
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    var body = await response.Content.ReadAsStringAsync();
        //    var documents = JsonConvert.DeserializeObject<Documents>(body);
        //    var included = documents.Included;
            
        //    Assert.Equal(4, included.Count);

        //    Assert.Equal(3, included.CountOfType("todo-items"));
        //    Assert.Equal(1, included.CountOfType("todo-collections"));
        //}

        //[Fact]
        //public async Task Can_Include_Nested_HasMany_Relationships_BelongsTo()
        //{
        //    // arrange
        //    const string route = "/api/v1/todo-items?include=collection.todo-items.owner";

        //    var todoItem = new TodoItem {
        //        Collection = new TodoItemCollection {
        //            Owner = new Person(),
        //            TodoItems = new List<TodoItem> {
        //                new TodoItem {
        //                    Owner = new Person()
        //                },
        //                new TodoItem()
        //            }
        //        }
        //    };
        
        //    var context = _fixture.GetService<AppDbContext>();
        //    ResetContext(context);

        //    context.TodoItems.Add(todoItem);
        //    await context.SaveChangesAsync();

        //    // act
        //    var response = await _fixture.Client.GetAsync(route);

        //    // assert
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    var body = await response.Content.ReadAsStringAsync();
        //    var documents = JsonConvert.DeserializeObject<Documents>(body);
        //    var included = documents.Included;
            
        //    Assert.Equal(5, included.Count);

        //    Assert.Equal(3, included.CountOfType("todo-items"));
        //    Assert.Equal(1, included.CountOfType("people"));
        //    Assert.Equal(1, included.CountOfType("todo-collections"));
        //}

        //[Fact]
        //public async Task Can_Include_Nested_Relationships_With_Multiple_Paths()
        //{
        //    // arrange
        //    const string route = "/api/v1/todo-items?include=collection.owner.role,collection.todo-items.owner";

        //    var todoItem = new TodoItem {
        //        Collection = new TodoItemCollection {
        //            Owner = new Person {
        //                Role = new PersonRole()
        //            },
        //            TodoItems = new List<TodoItem> {
        //                new TodoItem {
        //                    Owner = new Person()
        //                },
        //                new TodoItem()
        //            }
        //        }
        //    };
        
        //    var context = _fixture.GetService<AppDbContext>();
        //    ResetContext(context);

        //    context.TodoItems.Add(todoItem);
        //    await context.SaveChangesAsync();

        //    // act
        //    var response = await _fixture.Client.GetAsync(route);

        //    // assert
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    var body = await response.Content.ReadAsStringAsync();
        //    var documents = JsonConvert.DeserializeObject<Documents>(body);
        //    var included = documents.Included;
            
        //    Assert.Equal(7, included.Count);
            
        //    Assert.Equal(3, included.CountOfType("todo-items"));
        //    Assert.Equal(2, included.CountOfType("people"));
        //    Assert.Equal(1, included.CountOfType("person-roles"));
        //    Assert.Equal(1, included.CountOfType("todo-collections"));
        //}

        [Fact]
        public async Task Included_Resources_Are_Correct()
        {
            // arrange
            var role = new PersonRole();
            var assignee = new Person { Role = role };
            var collectionOwner = new Person();
            var someOtherOwner = new Person();
            var collection = new TodoItemCollection { Owner = collectionOwner };
            var todoItem1 = new TodoItem { Collection = collection, Assignee = assignee };
            var todoItem2 = new TodoItem { Collection = collection, Assignee = assignee };
            var todoItem3 = new TodoItem { Collection = collection, Owner = someOtherOwner };
            var todoItem4 = new TodoItem { Collection = collection, Owner = assignee };
        
            var context = _fixture.GetService<AppDbContext>();
            ResetContext(context);

            context.TodoItems.Add(todoItem1);
            context.TodoItems.Add(todoItem2);
            context.TodoItems.Add(todoItem3);
            context.TodoItems.Add(todoItem4);
            context.PersonRoles.Add(role);
            context.People.Add(assignee);
            context.People.Add(collectionOwner);
            context.People.Add(someOtherOwner);
            context.TodoItemCollections.Add(collection);


            await context.SaveChangesAsync();

            string route = 
                "/api/v1/todo-items/" + todoItem1.Id + "?include=" +
                    "collection.owner," + 
                    "assignee.role," + 
                    "assignee.assigned-todo-items";

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(body);
            var included = documents.Included;
            
            // 1 collection, 1 owner, 
            // 1 assignee, 1 assignee role,
            // 2 assigned todo items (including the primary resource)
            Assert.Equal(6, included.Count); 

            var collectionDocument = included.FindResource("todo-collections", collection.Id);
            var ownerDocument = included.FindResource("people", collectionOwner.Id);
            var assigneeDocument = included.FindResource("people", assignee.Id);
            var roleDocument = included.FindResource("person-roles", role.Id);
            var assignedTodo1 = included.FindResource("todo-items", todoItem1.Id);
            var assignedTodo2 = included.FindResource("todo-items", todoItem2.Id);

            Assert.NotNull(assignedTodo1);
            Assert.Equal(todoItem1.Id.ToString(), assignedTodo1.Id);

            Assert.NotNull(assignedTodo2);
            Assert.Equal(todoItem2.Id.ToString(), assignedTodo2.Id);

            Assert.NotNull(collectionDocument);
            Assert.Equal(collection.Id.ToString(), collectionDocument.Id);

            Assert.NotNull(ownerDocument);
            Assert.Equal(collectionOwner.Id.ToString(), ownerDocument.Id);

            Assert.NotNull(assigneeDocument);
            Assert.Equal(assignee.Id.ToString(), assigneeDocument.Id);

            Assert.NotNull(roleDocument);
            Assert.Equal(role.Id.ToString(), roleDocument.Id);
        }        

        [Fact]
        public async Task Can_Include_Doubly_HasMany_Relationships()
        {
            // arrange
            var person = new Person {
                TodoItemCollections = new List<TodoItemCollection> {
                    new TodoItemCollection {
                        TodoItems = new List<TodoItem> {
                            new TodoItem(),
                            new TodoItem()
                        }
                    },
                    new TodoItemCollection {
                        TodoItems = new List<TodoItem> {
                            new TodoItem(),
                            new TodoItem(),
                            new TodoItem()
                        }
                    }
                }
            };
        
            var context = _fixture.GetService<AppDbContext>();
            ResetContext(context);

            context.People.Add(person);

            await context.SaveChangesAsync();

            string route = "/api/v1/people/" + person.Id + "?include=todo-collections.todo-items";

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(body);
            var included = documents.Included;
            
            Assert.Equal(7, included.Count); 

            Assert.Equal(5, included.CountOfType("todo-items"));
            Assert.Equal(2, included.CountOfType("todo-collections"));
        }        
    }
}