using System;
using JsonApiDotNetCoreExample;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class AttributeSortTests
    {
        private readonly TestFixture<TestStartup> _fixture;

        public AttributeSortTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Cannot_Sort_If_Explicitly_Forbidden()
        {
            // Arrange
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems?include=owner&sort=achievedDate";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Sorting on the requested attribute is not allowed.", errorDocument.Errors[0].Title);
            Assert.Equal("Sorting on attribute 'achievedDate' is not allowed.", errorDocument.Errors[0].Detail);
            Assert.Equal("sort", errorDocument.Errors[0].Source.Parameter);
        }

        [Fact]
        public async Task Can_Sort_On_Multiple_Attributes()
        {
            // Arrange
            var category = Guid.NewGuid().ToString();

            var persons = new[]
            {
                new Person
                {
                    Category = category,
                    FirstName = "Alice",
                    LastName = "Smith",
                    Age = 23
                },
                new Person
                {
                    Category = category,
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 49
                },
                new Person
                {
                    Category = category,
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 31
                },
                new Person
                {
                    Category = category,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Age = 19
                }
            };

            _fixture.Context.People.AddRange(persons);
            _fixture.Context.SaveChanges();
            
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/people?filter[category]=" + category + "&sort=lastName,-firstName,the-Age";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.Equal(4, document.ManyData.Count);

            Assert.Equal(document.ManyData[0].Id, persons[2].StringId);
            Assert.Equal(document.ManyData[1].Id, persons[1].StringId);
            Assert.Equal(document.ManyData[2].Id, persons[3].StringId);
            Assert.Equal(document.ManyData[3].Id, persons[0].StringId);
        }
    }
}
