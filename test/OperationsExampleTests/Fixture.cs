using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace OperationsExampleTests
{
    public class Fixture : IDisposable
    {
        public Fixture()
        {
            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            Server = new TestServer(builder);
            Client = Server.CreateClient();
        }

        public TestServer Server { get; private set; }
        public HttpClient Client { get; }

        public void Dispose()
        {
            try
            {
                var context = GetService<AppDbContext>();
                context.Articles.RemoveRange(context.Articles);
                context.Authors.RemoveRange(context.Authors);
                context.SaveChanges();
            } // it is possible the test may try to do something that is an invalid db operation
              // validation should be left up to the test, so we should not bomb the run in the
              // disposal of that context
            catch (Exception) { }
        }

        public T GetService<T>() => (T)Server.Host.Services.GetService(typeof(T));

        public async Task<HttpResponseMessage> PatchAsync(string route, object data)
        {
            var httpMethod = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentLength = 1;
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
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
