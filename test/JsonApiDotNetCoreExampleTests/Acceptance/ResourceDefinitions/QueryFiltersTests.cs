using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    [Collection("WebHostCollection")]
    public sealed class QueryFiltersTests
    {
        private readonly TestFixture<TestStartup> _fixture;
        private readonly AppDbContext _context;
        private readonly Faker<User> _userFaker;

        public QueryFiltersTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _userFaker = new Faker<User>()
                .CustomInstantiator(f => new User(_context))
                .RuleFor(u => u.Username, f => f.Internet.UserName())
                .RuleFor(u => u.Password, f => f.Internet.Password());
        }

        [Fact]
        public async Task FiltersWithCustomQueryFiltersEquals()
        {
            // Arrange
            var user = _userFaker.Generate();
            var firstUsernameCharacter = user.Username[0];
            _context.Users.Add(user);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/users?filter[firstCharacter]=eq:{firstUsernameCharacter}";
            var request = new HttpRequestMessage(httpMethod, route);

            // @TODO - Use fixture
            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<User>(body).Data;
            Assert.True(deserializedBody.All(u => u.Username[0] == firstUsernameCharacter));
        }

        [Fact]
        public async Task FiltersWithCustomQueryFiltersLessThan()
        {
            // Arrange
            var aUser = _userFaker.Generate();
            aUser.Username = "alfred";
            var zUser = _userFaker.Generate();
            zUser.Username = "zac";
            _context.Users.AddRange(aUser, zUser);
            _context.SaveChanges();

            var median = 'h';

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/users?filter[firstCharacter]=lt:{median}";
            var request = new HttpRequestMessage(httpMethod, route);

            // @TODO - Use fixture
            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<User>(body).Data;
            Assert.True(deserializedBody.All(u => u.Username[0] < median));
        }
    }
}
