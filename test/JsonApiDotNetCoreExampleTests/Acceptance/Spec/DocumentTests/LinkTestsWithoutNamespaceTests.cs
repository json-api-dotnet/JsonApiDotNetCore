using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;
using Xunit;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    public sealed class LinkTestsWithoutNamespaceTests : FunctionalTestCollection<NoNamespaceApplicationFactory>
    {
        public LinkTestsWithoutNamespaceTests(NoNamespaceApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GET_RelativeLinks_True_Without_Namespace_Returns_RelativeLinks()
        {
            // Arrange
            var person = new Person();

            _dbContext.People.Add(person);
            _dbContext.SaveChanges();

            var route = "/people/" + person.StringId;
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            var options = (JsonApiOptions) _factory.GetService<IJsonApiOptions>();
            options.RelativeLinks = true;

            // Act
            var response = await _factory.Client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/people/" + person.StringId, document.Links.Self);
        }

        [Fact]
        public async Task GET_RelativeLinks_False_Without_Namespace_Returns_AbsoluteLinks()
        {
            // Arrange
            var person = new Person();

            _dbContext.People.Add(person);
            _dbContext.SaveChanges();

            var route = "/people/" + person.StringId;
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            var options = (JsonApiOptions) _factory.GetService<IJsonApiOptions>();
            options.RelativeLinks = false;

            // Act
            var response = await _factory.Client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal($"http://localhost/people/" + person.StringId, document.Links.Self);
        }
    }
}
