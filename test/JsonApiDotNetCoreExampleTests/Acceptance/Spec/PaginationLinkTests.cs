using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public class PaginationLinkTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        private const int _defaultPageSize = 5;

        private readonly Faker<TodoItem> _todoItemFaker = new Faker<TodoItem>();

        public PaginationLinkTests(StandardApplicationFactory factory) : base(factory)
        {
            var options = (JsonApiOptions) GetService<IJsonApiOptions>();

            options.DefaultPageSize = new PageSize(_defaultPageSize);
            options.MaximumPageSize = null;
            options.MaximumPageNumber = null;
            options.AllowUnknownQueryStringParameters = true;
        }

        [Theory]
        [InlineData(1, 1, 1, null, 2, 4)]
        [InlineData(2, 2, 1, 1, 3, 4)]
        [InlineData(3, 3, 1, 2, 4, 4)]
        [InlineData(4, 4, 1, 3, null, 4)]
        public async Task When_page_number_is_specified_it_must_display_correct_top_level_links(int pageNumber,
            int selfLink, int? firstLink, int? prevLink, int? nextLink, int? lastLink)
        {
            // Arrange
            const int totalCount = 18;

            var person = new Person
            {
                LastName = "&Ampersand"
            };

            var todoItems = _todoItemFaker.Generate(totalCount);
            foreach (var todoItem in todoItems)
            {
                todoItem.Owner = person;
            }

            await _dbContext.ClearTableAsync<TodoItem>();
            _dbContext.TodoItems.AddRange(todoItems);
            await _dbContext.SaveChangesAsync();

            string routePrefix = "/api/v1/todoItems?filter=equals(owner.lastName,'" + WebUtility.UrlEncode(person.LastName) + "')" +
                                 "&fields[owner]=firstName&include=owner&sort=ordinal&foo=bar,baz";
            string route = pageNumber != 1
                ? routePrefix + $"&page[size]={_defaultPageSize}&page[number]={pageNumber}"
                : routePrefix + $"&page[size]={_defaultPageSize}";

            // Act
            var response = await _client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var links = JsonConvert.DeserializeObject<Document>(body).Links;

            Assert.EndsWith($"{routePrefix}{GetPageNumberInQueryString(selfLink)}", links.Self);

            if (firstLink.HasValue)
            {
                var expected = $"{routePrefix}{GetPageNumberInQueryString(firstLink.Value)}";
                Assert.EndsWith(expected, links.First);
            }
            else
            {
                Assert.Null(links.First);
            }

            if (prevLink.HasValue)
            {
                var expected = $"{routePrefix}{GetPageNumberInQueryString(prevLink.Value)}";
                Assert.EndsWith(expected, links.Prev);
            }
            else
            {
                Assert.Null(links.Prev);
            }

            if (nextLink.HasValue)
            {
                var expected = $"{routePrefix}{GetPageNumberInQueryString(nextLink.Value)}";
                Assert.EndsWith(expected, links.Next);
            }
            else
            {
                Assert.Null(links.Next);
            }

            if (lastLink.HasValue)
            {
                var expected = $"{routePrefix}{GetPageNumberInQueryString(lastLink.Value)}";
                Assert.EndsWith(expected, links.Last);
            }
            else
            {
                Assert.Null(links.Last);
            }
        }

        private static string GetPageNumberInQueryString(int offset)
        {
            return offset == 1 ? string.Empty : $"&page[number]={offset}";
        }
    }
}
