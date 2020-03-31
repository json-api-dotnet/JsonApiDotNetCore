using System.Collections.Generic;
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
    public class EagerLoadTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        private readonly Faker<Person> _personFaker;
        private readonly Faker<Passport> _passportFaker;
        private readonly Faker<Country> _countryFaker;
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Visa> _visaFaker;

        public EagerLoadTests(StandardApplicationFactory factory) : base(factory)
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
            _visaFaker = new Faker<Visa>()
                .RuleFor(v => v.ExpiresAt, f => f.Date.Future());
        }

        [Fact]
        public async Task GetSingleResource_TopLevel_AppliesEagerLoad()
        {
            // Arrange
            var passport = _passportFaker.Generate();
            passport.BirthCountry = _countryFaker.Generate();
            
            var visa1 = _visaFaker.Generate();
            visa1.TargetCountry = _countryFaker.Generate();

            var visa2 = _visaFaker.Generate();
            visa2.TargetCountry = _countryFaker.Generate();

            passport.GrantedVisas = new List<Visa> { visa1, visa2 };

            _dbContext.Add(passport);
            _dbContext.SaveChanges();

            // Act
            var (body, response) = await Get($"/api/v1/passports/{passport.Id}");

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            
            var resultPassport = _deserializer.DeserializeSingle<PassportClient>(body).Data;
            Assert.Equal(passport.Id, resultPassport.Id);
            Assert.Equal(passport.BirthCountry.Name, resultPassport.BirthCountryName);
            Assert.Equal(visa1.TargetCountry.Name + ", " + visa2.TargetCountry.Name, resultPassport.GrantedVisaCountries);
        }

        [Fact]
        public async Task GetMultiResource_Nested_AppliesEagerLoad()
        {
            // Arrange
            var person = _personFaker.Generate();
            person.Passport = _passportFaker.Generate();
            person.Passport.BirthCountry = _countryFaker.Generate();

            _dbContext.People.RemoveRange(_dbContext.People);
            _dbContext.Add(person);
            _dbContext.SaveChanges();

            // Act
            var (body, response) = await Get($"/api/v1/people?include=passport");

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);

            var resultPerson = _deserializer.DeserializeList<Person>(body).Data.Single();
            Assert.Equal(person.Id, resultPerson.Id);
            Assert.Equal(person.Passport.Id, resultPerson.Passport.Id);
            Assert.Equal(person.Passport.BirthCountryName, resultPerson.Passport.BirthCountry.Name);
        } 
        
        [Fact]
        public async Task GetMultiResource_DeeplyNested_AppliesEagerLoad()
        {
            // Arrange
            var todo = _todoItemFaker.Generate();
            todo.Assignee = _personFaker.Generate();
            todo.Owner = _personFaker.Generate();;
            todo.Owner.Passport = _passportFaker.Generate();
            todo.Owner.Passport.BirthCountry = _countryFaker.Generate();

            _dbContext.Add(todo);
            _dbContext.SaveChanges();

            // Act
            var (body, response) = await Get($"/api/v1/people/{todo.Assignee.Id}/assignedTodoItems?include=owner.passport");

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);

            var resultTodoItem = _deserializer.DeserializeList<TodoItemClient>(body).Data.Single();
            Assert.Equal(todo.Owner.Passport.BirthCountryName, resultTodoItem.Owner.Passport.BirthCountry.Name);
        }

        [Fact]
        public async Task PostSingleResource_TopLevel_AppliesEagerLoad()
        {
            // Arrange
            var passport = _passportFaker.Generate();
            passport.BirthCountry = _countryFaker.Generate();

            var serializer = GetSerializer<Passport>(p => new { p.SocialSecurityNumber, p.BirthCountryName });
            var content = serializer.Serialize(passport);

            // Act
            var (body, response) = await Post($"/api/v1/passports", content);

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            
            var resultPassport = _deserializer.DeserializeSingle<PassportClient>(body).Data;
            Assert.Equal(passport.SocialSecurityNumber, resultPassport.SocialSecurityNumber);
            Assert.Equal(passport.BirthCountry.Name, resultPassport.BirthCountryName);
            Assert.Null(resultPassport.GrantedVisaCountries);
        }

        [Fact]
        public async Task PatchResource_TopLevel_AppliesEagerLoad()
        {
            // Arrange
            var passport = _passportFaker.Generate();
            passport.BirthCountry = _countryFaker.Generate();
            var visa = _visaFaker.Generate();
            visa.TargetCountry = _countryFaker.Generate();
            passport.GrantedVisas = new List<Visa> { visa };

            _dbContext.Add(passport);
            _dbContext.SaveChanges();

            passport.SocialSecurityNumber = _passportFaker.Generate().SocialSecurityNumber;
            passport.BirthCountry.Name = _countryFaker.Generate().Name;

            var serializer = GetSerializer<Passport>(p => new { p.SocialSecurityNumber, p.BirthCountryName });
            var content = serializer.Serialize(passport);

            // Act
            var (body, response) = await Patch($"/api/v1/passports/{passport.Id}", content);

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            
            var resultPassport = _deserializer.DeserializeSingle<PassportClient>(body).Data;
            Assert.Equal(passport.Id, resultPassport.Id);
            Assert.Equal(passport.SocialSecurityNumber, resultPassport.SocialSecurityNumber);
            Assert.Equal(passport.BirthCountry.Name, resultPassport.BirthCountryName);
            Assert.Equal(passport.GrantedVisas.First().TargetCountry.Name, resultPassport.GrantedVisaCountries);
        }
    }
}
