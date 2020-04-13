using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Acceptance.Spec;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public sealed class SerializationTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        public SerializationTests(StandardApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task When_getting_person_it_must_match_JSON_text()
        {
            // Arrange
            var person = new Person
            {
                Id = 123,
                FirstName = "John",
                LastName = "Doe",
                Age = 57,
                Gender = Gender.Male
            };

            _dbContext.People.RemoveRange(_dbContext.People);
            _dbContext.People.Add(person);
            _dbContext.SaveChanges();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/people/" + person.Id);

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var bodyText = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<JToken>(bodyText);
            var bodyFormatted = token.ToString().Replace("\r\n", "\n");
            
            Assert.Equal(@"{
  ""meta"": {
    ""copyright"": ""Copyright 2015 Example Corp."",
    ""authors"": [
      ""Jared Nance"",
      ""Maurits Moeys"",
      ""Harro van der Kroft""
    ]
  },
  ""links"": {
    ""self"": ""http://localhost/api/v1/people/123""
  },
  ""data"": {
    ""type"": ""people"",
    ""id"": ""123"",
    ""attributes"": {
      ""firstName"": ""John"",
      ""lastName"": ""Doe"",
      ""the-Age"": 57,
      ""gender"": ""Male""
    },
    ""relationships"": {
      ""todoItems"": {
        ""links"": {
          ""self"": ""http://localhost/api/v1/people/123/relationships/todoItems"",
          ""related"": ""http://localhost/api/v1/people/123/todoItems""
        }
      },
      ""assignedTodoItems"": {
        ""links"": {
          ""self"": ""http://localhost/api/v1/people/123/relationships/assignedTodoItems"",
          ""related"": ""http://localhost/api/v1/people/123/assignedTodoItems""
        }
      },
      ""todoCollections"": {
        ""links"": {
          ""self"": ""http://localhost/api/v1/people/123/relationships/todoCollections"",
          ""related"": ""http://localhost/api/v1/people/123/todoCollections""
        }
      },
      ""role"": {
        ""links"": {
          ""self"": ""http://localhost/api/v1/people/123/relationships/role"",
          ""related"": ""http://localhost/api/v1/people/123/role""
        }
      },
      ""oneToOneTodoItem"": {
        ""links"": {
          ""self"": ""http://localhost/api/v1/people/123/relationships/oneToOneTodoItem"",
          ""related"": ""http://localhost/api/v1/people/123/oneToOneTodoItem""
        }
      },
      ""stakeHolderTodoItem"": {
        ""links"": {
          ""self"": ""http://localhost/api/v1/people/123/relationships/stakeHolderTodoItem"",
          ""related"": ""http://localhost/api/v1/people/123/stakeHolderTodoItem""
        }
      },
      ""unIncludeableItem"": {
        ""links"": {
          ""self"": ""http://localhost/api/v1/people/123/relationships/unIncludeableItem"",
          ""related"": ""http://localhost/api/v1/people/123/unIncludeableItem""
        }
      },
      ""passport"": {
        ""links"": {
          ""self"": ""http://localhost/api/v1/people/123/relationships/passport"",
          ""related"": ""http://localhost/api/v1/people/123/passport""
        }
      }
    },
    ""links"": {
      ""self"": ""http://localhost/api/v1/people/123""
    }
  }
}", bodyFormatted);
        }
    }
}
