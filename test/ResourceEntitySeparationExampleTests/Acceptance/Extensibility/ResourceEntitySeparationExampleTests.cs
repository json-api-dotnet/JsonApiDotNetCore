using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace ResourceEntitySeparationExampleTests.Acceptance.Extensibility
{
    public class ResourceEntitySeparationExampleTests
    {
        private readonly TestServer _server;
        private readonly AppDbContext _context;

        private Faker<StudentEntity> _studentFaker;

        public ResourceEntitySeparationExampleTests()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TestStartup>();
            _server = new TestServer(builder);
            _context = _server.GetService<AppDbContext>();
            _context.Database.EnsureCreated();

            _studentFaker = new Faker<StudentEntity>()
                .RuleFor(s => s.FirstName, f => f.Name.FirstName())
                .RuleFor(s => s.LastName, f => f.Name.LastName());
        }

        [Fact]
        public async Task Can_Get_Students()
        {
            // arrange
            _context.Students.Add(_studentFaker.Generate());
            _context.SaveChanges();

            var client = _server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/students";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<StudentDto>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_Students_By_Id()
        {
            // arrange
            var student = _studentFaker.Generate();
            _context.Students.Add(student);
            _context.SaveChanges();

            var client = _server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/students/{student.Id}";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = (StudentDto)_server.GetService<IJsonApiDeSerializer>()
                .Deserialize(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.Equal(student.FirstName + " " + student.LastName, deserializedBody.Name);
        }

        [Fact]
        public async Task Can_Create_Students()
        {
            // arrange
            var student = _studentFaker.Generate();
            var client = _server.CreateClient();
            var httpMethod = new HttpMethod("POST");
            var route = $"/api/v1/students/";
            var content = new
            {
                data = new
                {
                    type = "students",
                    attributes = new Dictionary<string, string>()
                    {
                        {  "name", student.FirstName + " " + student.LastName }
                    }
                }
            };

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = (StudentDto)_server.GetService<IJsonApiDeSerializer>()
                .Deserialize(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.Equal(student.FirstName + " " + student.LastName, deserializedBody.Name);
        }
    }
}
