using System;
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
    public class AssertHelper
    {
        public static void HasEqualFieldValues<T>(T expected, T actual)
        {
            var failures = new List<string>();
            var fields = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                var v1 = field.GetValue(expected);
                var v2 = field.GetValue(actual);
                if (v1 == null && v2 == null) continue;
                if (!v1.Equals(v2)) failures.Add(string.Format("{0}: Expected:<{1}> Actual:<{2}>", field.Name, v1, v2));
            }
            if (failures.Any())
                Assert.True(false, "AssertHelper.HasEqualFieldValues failed. " + Environment.NewLine + string.Join(Environment.NewLine, failures));
        }
    }

    [Collection("WebHostCollection")]
    public class ResourceDefinitionTests
    {
        private TestFixture<TestStartup> _fixture;
        private AppDbContext _context;
        private Faker<User> _userFaker;
        private static readonly Faker<Article> _articleFaker = new Faker<Article>()
            .RuleFor(a => a.Name, f => f.Random.AlphaNumeric(10))
            .RuleFor(a => a.Author, f => new Author());

        private static readonly Faker<Tag> _tagFaker = new Faker<Tag>().RuleFor(a => a.Name, f => f.Random.AlphaNumeric(10));
        public ResourceDefinitionTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _userFaker = new Faker<User>()
                .RuleFor(u => u.Username, f => f.Internet.UserName())
                .RuleFor(u => u.Password, f => f.Internet.Password());
        }

        [Fact]
        public async Task Tag_Is_Hidden()
        {
            // Arrange
            var context = _fixture.GetService<AppDbContext>();
            var article = _articleFaker.Generate();
            var tags = _tagFaker.Generate(2);

            tags[0].Name = "THISTAGSHOULDNOTBEVISIBLE";

            context.Articles.RemoveRange(context.Articles);
            await context.SaveChangesAsync();

            var articleTags = new ArticleTag[]
            {
                new ArticleTag
                {
                    Article = article,
                    Tag = tags[0]
                },
                new ArticleTag
                {
                    Article = article,
                    Tag = tags[1]
                }
            };
            context.ArticleTags.AddRange(articleTags);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles?include=tags";

            var httpMethod = new HttpMethod("GET");
            var request = new HttpRequestMessage(httpMethod, route);


            // Act
            var response = await _fixture.Client.GetAsync(route);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");

            var document = JsonConvert.DeserializeObject<Documents>(body);
            Assert.NotEmpty(document.Included);

            var articleResponseList = _fixture.GetService<IJsonApiDeSerializer>().DeserializeList<Article>(body);
            Assert.NotNull(articleResponseList);

            var articleResponse = articleResponseList.FirstOrDefault(a => a.Id == article.Id);
            Assert.NotNull(articleResponse);
            Assert.Equal(article.Name, articleResponse.Name);


            AssertHelper.HasEqualFieldValues(articleResponse.Tags[0], tags[1]);
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
