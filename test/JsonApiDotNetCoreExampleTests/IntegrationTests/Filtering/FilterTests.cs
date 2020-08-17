using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Filtering
{
    public sealed class FilterTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        public FilterTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.EnableLegacyFilterNotation = false;
        }

        [Fact]
        public async Task Cannot_filter_in_unknown_scope()
        {
            // Arrange
            var route = "/api/v1/people?filter[doesNotExist]=equals(title,null)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified filter is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' does not exist on resource 'people'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter[doesNotExist]");
        }

        [Fact]
        public async Task Cannot_filter_in_unknown_nested_scope()
        {
            // Arrange
            var route = "/api/v1/people?filter[todoItems.doesNotExist]=equals(title,null)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified filter is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' in 'todoItems.doesNotExist' does not exist on resource 'todoItems'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter[todoItems.doesNotExist]");
        }

        [Fact]
        public async Task Cannot_filter_on_blocked_attribute()
        {
            // Arrange
            var route = "/api/v1/todoItems?filter=equals(achievedDate,null)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Filtering on the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Filtering on attribute 'achievedDate' is not allowed.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter");
        }

        [Fact]
        public async Task Can_filter_on_ID()
        {
            // Arrange
            var person = new Person
            {
                FirstName = "Jane"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Person>();
                dbContext.People.AddRange(person, new Person());

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/people?filter=equals(id,'{person.StringId}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(person.StringId);
            responseDocument.ManyData[0].Attributes["firstName"].Should().Be(person.FirstName);
        }

        [Fact]
        public async Task Can_filter_on_obfuscated_ID()
        {
            // Arrange
            Passport passport = null;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                passport = new Passport(dbContext)
                {
                    SocialSecurityNumber = 123,
                    BirthCountry = new Country()
                };

                await dbContext.ClearTableAsync<Passport>();
                dbContext.Passports.AddRange(passport, new Passport(dbContext));

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/passports?filter=equals(id,'{passport.StringId}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(passport.StringId);
            responseDocument.ManyData[0].Attributes["socialSecurityNumber"].Should().Be(passport.SocialSecurityNumber);
        }

        [Fact]
        public async Task Can_filter_in_set_on_obfuscated_ID()
        {
            // Arrange
            var passports = new List<Passport>();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                passports.AddRange(new[]
                {
                    new Passport(dbContext)
                    {
                        SocialSecurityNumber = 123,
                        BirthCountry = new Country()
                    },
                    new Passport(dbContext)
                    {
                        SocialSecurityNumber = 456,
                        BirthCountry = new Country()
                    },
                    new Passport(dbContext)
                    {
                        BirthCountry = new Country()
                    }
                });

                await dbContext.ClearTableAsync<Passport>();
                dbContext.Passports.AddRange(passports);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/passports?filter=any(id,'{passports[0].StringId}','{passports[1].StringId}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            
            responseDocument.ManyData[0].Id.Should().Be(passports[0].StringId);
            responseDocument.ManyData[0].Attributes["socialSecurityNumber"].Should().Be(passports[0].SocialSecurityNumber);

            responseDocument.ManyData[1].Id.Should().Be(passports[1].StringId);
            responseDocument.ManyData[1].Attributes["socialSecurityNumber"].Should().Be(passports[1].SocialSecurityNumber);
        }
    }
}
