using Bogus;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Contracts;

using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models.Entities;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ResourceEntitySeparationExampleTests
{
    public class TestFixture : IDisposable
    {
        public HttpClient Client { get; private set; }
        public AppDbContext Context { get; private set; }
        public TestServer Server { get; private set; }

        public Faker<CourseEntity> CourseFaker { get; private set; }
        public Faker<DepartmentEntity> DepartmentFaker { get; private set; }
        public Faker<StudentEntity> StudentFaker { get; private set; }

        public TestFixture()
        {
            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            Server = new TestServer(builder);
            Context = Server.GetService<AppDbContext>();
            Context.Database.EnsureCreated();
            Client = Server.CreateClient();

            CourseFaker = new Faker<CourseEntity>()
                .RuleFor(c => c.Number, f => f.Random.Int(min: 0, max: 1000))
                .RuleFor(c => c.Title, f => f.Name.JobArea())
                .RuleFor(c => c.Description, f => f.Lorem.Paragraph());

            DepartmentFaker = new Faker<DepartmentEntity>()
                .RuleFor(d => d.Name, f => f.Commerce.Department());

            StudentFaker = new Faker<StudentEntity>()
                .RuleFor(s => s.FirstName, f => f.Name.FirstName())
                .RuleFor(s => s.LastName, f => f.Name.LastName());
        }

        public void Dispose()
        {
            Server.Dispose();
        }

        public async Task<HttpResponseMessage> DeleteAsync(string route)
        {
            return await SendAsync("DELETE", route, null);
        }

        public async Task<(HttpResponseMessage response, T data)> GetAsync<T>(string route)
        {
            return await SendAsync<T>("GET", route, null);
        }

        public async Task<(HttpResponseMessage response, T data)> PatchAsync<T>(string route, object data)
        {
            return await SendAsync<T>("PATCH", route, data);
        }

        public async Task<(HttpResponseMessage response, T data)> PostAsync<T>(string route, object data)
        {
            return await SendAsync<T>("POST", route, data);
        }

        public async Task<HttpResponseMessage> SendAsync(string method, string route, object data)
        {
            var httpMethod = new HttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(data))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            return await Client.SendAsync(request);
        }

        public async Task<(HttpResponseMessage response, T data)> SendAsync<T>(string method, string route, object data)
        {
            var response = await SendAsync(method, route, data);
            var json = await response.Content.ReadAsStringAsync();
            var obj = (T)Server.GetService<IJsonApiDeserializer>().Deserialize(json);
            return (response, obj);
        }
    }
}
