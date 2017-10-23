using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using OperationsExample;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Formatters;

namespace OperationsExampleTests
{
    [CollectionDefinition("WebHostCollection")]
    public class WebHostCollection : ICollectionFixture<Fixture>
    { }

    public class Fixture
    {
        public Fixture()
        {
            var builder = new WebHostBuilder().UseStartup<Startup>();
            Server = new TestServer(builder);
            Client = Server.CreateClient();
        }

        public TestServer Server { get; private set; }
        public HttpClient Client { get; }
        public T GetService<T>() => (T)Server.Host.Services.GetService(typeof(T));

        public async Task<HttpResponseMessage> PatchAsync(string route, object data)
        {
            var httpMethod = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentLength = 1;
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            request.Content.Headers.Add("Link", JsonApiOperationsInputFormatter.PROFILE_EXTENSION);
            return await Client.SendAsync(request);
        }

        public async Task<(HttpResponseMessage response, T data)> PatchAsync<T>(string route, object data)
        {
            var response = await PatchAsync(route, data);
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<T>(json);
            return (response, obj);
        }
    }
}