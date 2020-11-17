using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    public sealed class LinksWithNamespaceTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        public LinksWithNamespaceTests(StandardApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GET_RelativeLinks_False_With_Namespace_Returns_AbsoluteLinks()
        {
            // Arrange
            var person = new Person();

            _dbContext.People.Add(person);
            await _dbContext.SaveChangesAsync();

            var route = "/api/v1/people/" + person.StringId;
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            var options = (JsonApiOptions) _factory.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = false;

            // Act
            var response = await _factory.Client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("http://localhost/api/v1/people/" + person.StringId, document.Links.Self);
        }
    }
}
