using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Models;
using System.Collections;
using JsonApiDotNetCoreExample;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    [Collection("WebHostCollection")]
    public class RequestMetaTests
    {
        private TestFixture<Startup> _fixture;

        public RequestMetaTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Injecting_IRequestMeta_Adds_Meta_Data()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .UseStartup<MetaStartup>();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/people";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var expectedMeta = (_fixture.GetService<ResourceDefinition<Person>>() as IHasMeta).GetMeta();

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var meta = _fixture.GetDeserializer().DeserializeList<Person>(body).Meta;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(meta);
            Assert.NotNull(expectedMeta);
            Assert.NotEmpty(expectedMeta);

            foreach (var hash in expectedMeta)
            {
                if (hash.Value is IList)
                {
                    var listValue = (IList)hash.Value;
                    for (var i = 0; i < listValue.Count; i++)
                        Assert.Equal(listValue[i].ToString(), ((IList)meta[hash.Key])[i].ToString());
                }
                else
                {
                    Assert.Equal(hash.Value, meta[hash.Key]);
                }
            }
            Assert.Equal("request-meta-value", meta["request-meta"]);
        }
    }
}
