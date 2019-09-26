using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Contracts;

using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    [Collection("WebHostCollection")]
    public class QueryFiltersTests
    {
      private TestFixture<TestStartup> _fixture;
      private AppDbContext _context;
      private Faker<User> _userFaker;

      public QueryFiltersTests(TestFixture<TestStartup> fixture)
      {
        _fixture = fixture;
        _context = fixture.GetService<AppDbContext>();
        _userFaker = new Faker<User>()
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
        var route = $"/api/v1/users?filter[first-character]=eq:{firstUsernameCharacter}";
        var request = new HttpRequestMessage(httpMethod, route);

        // Act
        var response = await _fixture.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var deserializedBody = _fixture.GetService<IJsonApiDeserializer>().DeserializeList<User>(body);
        var usersWithFirstCharacter = _context.Users.Where(u => u.Username[0] == firstUsernameCharacter);
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
        var route = $"/api/v1/users?filter[first-character]=lt:{median}";
        var request = new HttpRequestMessage(httpMethod, route);

        // Act
        var response = await _fixture.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var deserializedBody = _fixture.GetService<IJsonApiDeserializer>().DeserializeList<User>(body);
        Assert.True(deserializedBody.All(u => u.Username[0] < median));
      }
    }
}
