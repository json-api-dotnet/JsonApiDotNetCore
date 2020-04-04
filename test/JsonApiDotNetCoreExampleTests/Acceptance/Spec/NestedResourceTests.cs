using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public sealed class NestedResourceTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;
        private readonly Faker<Passport> _passportFaker;
        private readonly Faker<Country> _countryFaker;

        public NestedResourceTests(StandardApplicationFactory factory) : base(factory)
        {
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
            _personFaker = new Faker<Person>()
                .RuleFor(t => t.FirstName, f => f.Name.FirstName())
                .RuleFor(t => t.LastName, f => f.Name.LastName());
            _passportFaker = new Faker<Passport>()
                .RuleFor(t => t.SocialSecurityNumber, f => f.Random.Number(100, 10_000));
            _countryFaker = new Faker<Country>()
                .RuleFor(c => c.Name, f => f.Address.Country());
        }

        [Fact]
        public async Task NestedResourceRoute_RequestWithIncludeQueryParam_ReturnsRequestedRelationships()
        {
            // Arrange
            var todo = _todoItemFaker.Generate();
            todo.Assignee = _personFaker.Generate();
            todo.Owner = _personFaker.Generate();
            todo.Owner.Passport = _passportFaker.Generate();
            todo.Owner.Passport.BirthCountry = _countryFaker.Generate();

            _dbContext.Add(todo);
            _dbContext.SaveChanges();

            // Act
            var (body, response) = await Get($"/api/v1/people/{todo.Assignee.Id}/assignedTodoItems?include=owner.passport");

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            var resultTodoItem = _deserializer.DeserializeList<TodoItemClient>(body).Data.Single();
            Assert.Equal(todo.Id, resultTodoItem.Id);
            Assert.Equal(todo.Owner.Id, resultTodoItem.Owner.Id);
            Assert.Equal(todo.Owner.Passport.Id, resultTodoItem.Owner.Passport.Id);
        }

        [Theory]
        [InlineData("filter[ordinal]=1")]
        [InlineData("fields=ordinal")]
        [InlineData("sort=ordinal")]
        [InlineData("page[number]=1")]
        [InlineData("page[size]=10")]
        public async Task NestedResourceRoute_RequestWithUnsupportedQueryParam_ReturnsBadRequest(string queryParameter)
        {
            string parameterName = queryParameter.Split('=')[0];

            // Act
            var (body, response) = await Get($"/api/v1/people/1/assignedTodoItems?{queryParameter}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified query string parameter is currently not supported on nested resource endpoints.", errorDocument.Errors[0].Title);
            Assert.Equal($"Query string parameter '{parameterName}' is currently not supported on nested resource endpoints. (i.e. of the form '/article/1/author?parameterName=...')", errorDocument.Errors[0].Detail);
            Assert.Equal(parameterName, errorDocument.Errors[0].Source.Parameter);
        }
    }
}
