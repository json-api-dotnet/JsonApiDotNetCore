using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
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
        public async Task NestedResourceRoute_RequestWithUnsupportedQueryParam_ReturnsBadRequest(string queryParam)
        {
            // Act
            var (body, response) = await Get($"/api/v1/people/1/assignedTodoItems?{queryParam}");

            // Assert
            AssertEqualStatusCode(HttpStatusCode.BadRequest, response);
            Assert.Contains("currently not supported", body);
        }
    }
}
