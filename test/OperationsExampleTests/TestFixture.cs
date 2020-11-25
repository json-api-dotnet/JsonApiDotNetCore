using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace OperationsExampleTests
{
    public class TestFixture<TStartup> : IDisposable
        where TStartup : class
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private bool _isDisposed;

        public AppDbContext Context { get; }

        public TestFixture()
        {
            var builder = WebHost.CreateDefaultBuilder().UseStartup<TStartup>();
            _server = new TestServer(builder);

            _client = _server.CreateClient();

            var dbContextResolver = _server.Host.Services.GetRequiredService<IDbContextResolver>();
            Context = (AppDbContext) dbContextResolver.GetContext();
        }

        public void AssertEqualStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            var responseBody = response.Content.ReadAsStringAsync().Result;
            Assert.True(expected == response.StatusCode,
                $"Got {response.StatusCode} status code instead of {expected}. Response body: {responseBody}");
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                _client.Dispose();
                _server.Dispose();
            }
        }

        public async Task<(HttpResponseMessage response, T data)> PostAsync<T>(string route, object data)
        {
            var response = await PostAsync(route, data);
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<T>(json);
            return (response, obj);
        }

        private async Task<HttpResponseMessage> PostAsync(string route, object data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, route);

            if (data != null)
            {
                var requestBody = JsonConvert.SerializeObject(data);
                requestBody = requestBody.Replace("__", ":");

                request.Content = new StringContent(requestBody);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);
            }

            return await _client.SendAsync(request);
        }
    }
}
