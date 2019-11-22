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
    public class NestedResourceTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;
        private readonly Faker<Passport> _passportFaker;

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
                    .RuleFor(t => t.SocialSecurityNumber, f => f.Random.Number());
        }

        [Fact]
        public async Task NestedResourceRoute_IncludeDeeplyNestedRelationship_ReturnsRequestedRelationships()
        {
            // Arrange
            var assignee = _dbContext.Add(_personFaker.Generate()).Entity;
            var todo = _dbContext.Add(_todoItemFaker.Generate()).Entity;
            var owner = _dbContext.Add(_personFaker.Generate()).Entity;
            var passport = _dbContext.Add(_passportFaker.Generate()).Entity;
            _dbContext.SaveChanges();
            todo.AssigneeId = assignee.Id;
            todo.OwnerId = owner.Id;
            owner.PassportId = passport.Id;
            _dbContext.SaveChanges();

            // Act
            var (body, response) = await Get($"/api/v1/people/{assignee.Id}/assignedTodoItems?include=owner.passport");

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            var resultTodoItem = _deserializer.DeserializeList<TodoItemClient>(body).Data.SingleOrDefault();
            Assert.Equal(todo.Id, resultTodoItem.Id);
            Assert.Equal(todo.Owner.Id, resultTodoItem.Owner.Id);
            Assert.Equal(todo.Owner.Passport.Id, resultTodoItem.Owner.Passport.Id);
        }
    }
}
