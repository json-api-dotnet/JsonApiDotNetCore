using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
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
            context.TodoItems.RemoveRange(context.TodoItems);
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var todoItems = _fixture.DeSerializer.DeserializeList<TodoItem>(body);

            var responseTodoItem = todoItems[0];
            Assert.NotNull(responseTodoItem);
            Assert.NotNull(responseTodoItem.Collection);
            Assert.NotNull(responseTodoItem.Collection.TodoItems);
            Assert.Equal(2, responseTodoItem.Collection.TodoItems.Count);

            // TODO: assert number of things in included
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
            context.TodoItems.RemoveRange(context.TodoItems);
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var todoItems = _fixture.DeSerializer.DeserializeList<TodoItem>(body);

            var responseTodoItem = todoItems[0];
            Assert.NotNull(responseTodoItem);
            Assert.NotNull(responseTodoItem.Collection);
            Assert.NotNull(responseTodoItem.Collection.TodoItems);
            Assert.Equal(2, responseTodoItem.Collection.TodoItems.Count);

            // TODO: assert number of things in included
        }
    }
}