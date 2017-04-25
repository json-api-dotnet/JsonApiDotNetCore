using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetCoreDocs;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Data;
using Bogus;
using JsonApiDotNetCoreExample.Models;
using System;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public class PagingTests
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private AppDbContext _context;
        private Faker<Person> _personFaker;
        private Faker<TodoItem> _todoItemFaker;
        private Faker<TodoItemCollection> _todoItemCollectionFaker;

        public PagingTests(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
				.RuleFor(t => t.CreatedDate, f => f.Date.Past());

            _todoItemCollectionFaker = new Faker<TodoItemCollection>()
                .RuleFor(t => t.Name, f => f.Company.CatchPhrase());
        }

        [Fact]
        public async Task Server_IncludesPagination_Links()
        {
            // arrange
            var pageSize = 5;
            const int minimumNumberOfRecords = 11;
            _context.TodoItems.RemoveRange(_context.TodoItems);

            for(var i=0; i < minimumNumberOfRecords; i++)
                _context.TodoItems.Add(_todoItemFaker.Generate());

            await _context.SaveChangesAsync();

            var numberOfPages = (int)Math.Ceiling(decimal.Divide(minimumNumberOfRecords, pageSize));
            var startPageNumber = 2;

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?page[number]=2";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Documents>(await response.Content.ReadAsStringAsync());
            var links = documents.Links;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(links.First);
            Assert.NotEmpty(links.Next);
            Assert.NotEmpty(links.Last);

            Assert.Equal($"http://localhost/api/v1/todo-items?page[size]={pageSize}&page[number]={startPageNumber+1}", links.Next);
            Assert.Equal($"http://localhost/api/v1/todo-items?page[size]={pageSize}&page[number]={startPageNumber-1}", links.Prev);
            Assert.Equal($"http://localhost/api/v1/todo-items?page[size]={pageSize}&page[number]={numberOfPages}", links.Last);
            Assert.Equal($"http://localhost/api/v1/todo-items?page[size]={pageSize}&page[number]=1", links.First);
        }
    }
}
