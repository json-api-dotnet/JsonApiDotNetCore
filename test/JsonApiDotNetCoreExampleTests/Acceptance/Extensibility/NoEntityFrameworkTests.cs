using JsonApiDotNetCore.Serialization;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NoEntityFrameworkExample;
using System.Net.Http;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using NoEntityFrameworkExample.Models;
using System.Threading.Tasks;
using System.Net;
using System;
using Newtonsoft.Json;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    public class NoEntityFrameworkTests
    {
        [Fact]
        public async Task Can_Implement_Custom_IResourceService_Without_EFAsync()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/my-models";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<MyModel>(responseBody);
                
            var expectedBody = JsonConvert.SerializeObject(new Documents {
                Data = new List<DocumentData> {
                    new DocumentData {
                        Id = "1",
                        Type = "my-models",
                        Attributes = new Dictionary<string, object> {
                            {"description", "description"}
                        }
                    }
                }
            }, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            })
            .Replace(" ", string.Empty)
            .ToLower();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedBody, responseBody);
        }
    }
}
