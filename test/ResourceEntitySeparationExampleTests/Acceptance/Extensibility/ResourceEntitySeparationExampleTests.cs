using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using Newtonsoft.Json;
using Xunit;

namespace ResourceEntitySeparationExampleTests.Acceptance.Extensibility
{
    public class ResourceEntitySeparationExampleTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public ResourceEntitySeparationExampleTests(TestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public async Task Can_Get_Students()
        {
            // arrange
            _fixture.Context.Students.Add(_fixture.StudentFaker.Generate());
            _fixture.Context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/students";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Server.CreateClient().SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeSerializer>()
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
            var student = _fixture.StudentFaker.Generate();
            _fixture.Context.Students.Add(student);
            _fixture.Context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/students/{student.Id}";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Server.CreateClient().SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = (StudentDto)_fixture.Server.GetService<IJsonApiDeSerializer>()
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
            var student = _fixture.StudentFaker.Generate();
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
            var response = await _fixture.Server.CreateClient().SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = (StudentDto)_fixture.Server.GetService<IJsonApiDeSerializer>()
                .Deserialize(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.Equal(student.FirstName + " " + student.LastName, deserializedBody.Name);
        }
    }
}
