using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    public sealed class Links : FunctionalTestCollection<StandardApplicationFactory>
        {
            public Links(StandardApplicationFactory factory) : base(factory) { }

            [Fact]
            public async Task GET_RelativeLinks_True_Returns_RelativeLinks()
            {
                // Arrange
                var person = new Person
                {
                    Id = 123,
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 57,
                    Gender = Gender.Male,
                    Category = "Family"
                };

                _dbContext.People.RemoveRange(_dbContext.People);
                _dbContext.People.Add(person);
                _dbContext.SaveChanges();

                var httpMethod = new HttpMethod("GET");
                var route = $"http://localhost/api/v1/people/{person.Id}";

                var request = new HttpRequestMessage(httpMethod, route);

                var options = (JsonApiOptions) _factory.GetService<IJsonApiOptions>();
                options.RelativeLinks = true;

                // Act
                var response = await _factory.Client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                var document = JsonConvert.DeserializeObject<Document>(responseString);
                var expectedOwnerSelfLink = $"/api/v1/people/{person.Id}";

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(expectedOwnerSelfLink, document.Links.Self);
            }

            [Fact]
            public async Task GET_RelativeLinks_False_Returns_HostsLinks()
            {
                // Arrange
                var person = new Person
                {
                    Id = 123,
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 57,
                    Gender = Gender.Male,
                    Category = "Family"
                };

                _dbContext.People.RemoveRange(_dbContext.People);
                _dbContext.People.Add(person);
                _dbContext.SaveChanges();

                var httpMethod = new HttpMethod("GET");
                var route = $"http://localhost/api/v1/people/{person.Id}";

                var request = new HttpRequestMessage(httpMethod, route);

                var options = (JsonApiOptions) _factory.GetService<IJsonApiOptions>();
                options.RelativeLinks = false;

                // Act
                var response = await _factory.Client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                var document = JsonConvert.DeserializeObject<Document>(responseString);
                var expectedOwnerSelfLink = $"http://localhost/api/v1/people/{person.Id}";

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(expectedOwnerSelfLink, document.Links.Self);
            }
        }
}