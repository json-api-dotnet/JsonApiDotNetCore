using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    [Collection("WebHostCollection")]
    public class OutputAttrs_Tests
    {
        private TestFixture<TestStartup> _fixture;
        private AppDbContext _context;
        private Faker<User> _userFaker;

        public OutputAttrs_Tests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _userFaker = new Faker<User>()
                .RuleFor(u => u.Username, f => f.Internet.UserName())
                .RuleFor(u => u.Password, f => f.Internet.Password());
        }

        [Fact]
        public async Task Password_Is_Not_Included_In_Response_Payload()
        {
            // Arrange
            var user = _userFaker.Generate();
            _context.Users.Add(user);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/users/{user.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.False(document.Data.Attributes.ContainsKey("password"));
        }

        [Fact]
        public async Task Can_Create_User_With_Password()
        {
            // Arrange
            var user = _userFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "users",
                    attributes = new Dictionary<string, object>()
                    {
                        { "username", user.Username },
                        { "password", user.Password },
                    }
                }
            };

            var httpMethod = new HttpMethod("POST");
            var route = $"/api/v1/users";

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // response assertions
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (User)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.False(document.Data.Attributes.ContainsKey("password"));
            Assert.Equal(user.Username, document.Data.Attributes["username"]);

            // db assertions
            var dbUser = await _context.Users.FindAsync(deserializedBody.Id);
            Assert.Equal(user.Username, dbUser.Username);
            Assert.Equal(user.Password, dbUser.Password);
        }

        [Fact]
        public async Task Can_Update_User_Password()
        {
            // Arrange
            var user = _userFaker.Generate();
            _context.Users.Add(user);
            _context.SaveChanges();

            var newPassword = _userFaker.Generate().Password;

            var content = new
            {
                data = new
                {
                    type = "users",
                    id = user.Id,
                    attributes = new Dictionary<string, object>()
                    {
                        { "password", newPassword },
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/users/{user.Id}";

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // response assertions
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (User)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.False(document.Data.Attributes.ContainsKey("password"));
            Assert.Equal(user.Username, document.Data.Attributes["username"]);

            // db assertions
            var dbUser = _context.Users.AsNoTracking().Single(u => u.Id == user.Id);
            Assert.Equal(newPassword, dbUser.Password);
        }
    }
}
