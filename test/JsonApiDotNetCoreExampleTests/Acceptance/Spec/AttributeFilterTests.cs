using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class AttributeFilterTests
    {
        private TestFixture<TestStartup> _fixture;
        private Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public AttributeFilterTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());

            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());
        }

        [Fact]
        public async Task Can_Filter_On_Guid_Properties()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?filter[guid-property]={todoItem.GuidProperty}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var list =  _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;
 

            var todoItemResponse = list.Single();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(todoItem.Id, todoItemResponse.Id);
            Assert.Equal(todoItem.GuidProperty, todoItemResponse.GuidProperty);
        }

        [Fact]
        public async Task Can_Filter_On_Related_Attrs()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?include=owner&filter[owner.first-name]={person.FirstName}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var list = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data.First();


            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            list.Owner.FirstName = person.FirstName;
        }

        [Fact]
        public async Task Cannot_Filter_If_Explicitly_Forbidden()
        {
            // arrange
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?include=owner&filter[achieved-date]={DateTime.UtcNow.Date}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_Filter_On_Not_Equal_Values()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var totalCount = context.TodoItems.Count();
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?page[size]={totalCount}&filter[ordinal]=ne:{todoItem.Ordinal}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var list = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain(list, x => x.Ordinal == todoItem.Ordinal);
        }

        [Fact]
        public async Task Can_Filter_On_In_Array_Values()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItems = _todoItemFaker.Generate(5);
            var guids = new List<Guid>();
            var notInGuids = new List<Guid>();
            foreach (var item in todoItems)
            {
                context.TodoItems.Add(item);
                // Exclude 2 items
                if (guids.Count < (todoItems.Count() - 2))
                    guids.Add(item.GuidProperty);
                else 
                    notInGuids.Add(item.GuidProperty);
            }
            context.SaveChanges();

            var totalCount = context.TodoItems.Count();
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?filter[guid-property]=in:{string.Join(",", guids)}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedTodoItems = _fixture
                .GetDeserializer()
                .DeserializeList<TodoItem>(body).Data;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(guids.Count(), deserializedTodoItems.Count());
            foreach (var item in deserializedTodoItems)
            {
                Assert.Contains(item.GuidProperty, guids);
                Assert.DoesNotContain(item.GuidProperty, notInGuids);
            }
        }

        [Fact]
        public async Task Can_Filter_On_Related_In_Array_Values()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItems = _todoItemFaker.Generate(3);
            var ownerFirstNames = new List<string>();
            foreach (var item in todoItems)
            {
                var person = _personFaker.Generate();
                ownerFirstNames.Add(person.FirstName);
                item.Owner = person;
                context.TodoItems.Add(item);               
            }
            context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?include=owner&filter[owner.first-name]=in:{string.Join(",", ownerFirstNames)}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());
            var included = documents.Included;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ownerFirstNames.Count(), documents.ManyData.Count());
            Assert.NotNull(included);
            Assert.NotEmpty(included);
            foreach (var item in included)
                Assert.Contains(item.Attributes["first-name"], ownerFirstNames);

        }

        [Fact]
        public async Task Can_Filter_On_Not_In_Array_Values()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.RemoveRange(context.TodoItems);
            context.SaveChanges();
            var todoItems = _todoItemFaker.Generate(5);
            var guids = new List<Guid>();
            var notInGuids = new List<Guid>();
            foreach (var item in todoItems)
            {
                context.TodoItems.Add(item);
                // Exclude 2 items
                if (guids.Count < (todoItems.Count() - 2))
                    guids.Add(item.GuidProperty);
                else
                    notInGuids.Add(item.GuidProperty);
            }
            context.SaveChanges();

            var totalCount = context.TodoItems.Count();
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?page[size]={totalCount}&filter[guid-property]=nin:{string.Join(",", notInGuids)}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedTodoItems = _fixture
                .GetDeserializer()
                .DeserializeList<TodoItem>(body).Data;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(totalCount - notInGuids.Count(), deserializedTodoItems.Count());
            foreach (var item in deserializedTodoItems)
            {
                Assert.DoesNotContain(item.GuidProperty, notInGuids);
            }
        }
    }
}
