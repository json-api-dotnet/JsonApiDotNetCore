using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Contracts;

using JsonApiDotNetCoreExample.Models.Entities;
using JsonApiDotNetCoreExample.Models.Resources;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ResourceEntitySeparationExampleTests.Acceptance
{
    [Collection("TestCollection")]
    public class GetTests
    {
        private readonly TestFixture _fixture;

        public GetTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Get_Courses()
        {
            // arrange
            var course = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/courses";

            // act
            var response = await _fixture.SendAsync("GET", route, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeserializer>()
                .DeserializeList<CourseResource>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_Course_By_Id()
        {
            // arrange
            var course = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/courses/{course.Id}";

            // act
            var (response, data) = await _fixture.GetAsync<CourseResource>(route);
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(course.Number, data.Number);
            Assert.Equal(course.Title, data.Title);
            Assert.Equal(course.Description, data.Description);
        }

        [Fact]
        public async Task Can_Get_Course_With_Relationships()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            _fixture.Context.Departments.Add(dept);
           
            var course = _fixture.CourseFaker.Generate();
            course.Department = dept;
            _fixture.Context.Courses.Add(course);
            
            var student = _fixture.StudentFaker.Generate();
            _fixture.Context.Students.Add(student);
            
            var reg = new CourseStudentEntity(course, student);
            _fixture.Context.Registrations.Add(reg);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/courses/{course.Id}?include=students,department";

            // act
            var (response, data) = await _fixture.GetAsync<CourseResource>(route);
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);

            Assert.Equal(course.Number, data.Number);
            Assert.Equal(course.Title, data.Title);
            Assert.Equal(course.Description, data.Description);

            Assert.NotEmpty(data.Students);
            Assert.NotNull(data.Students[0]);
            Assert.Equal(student.Id, data.Students[0].Id);
            Assert.Equal(student.LastName, data.Students[0].LastName);

            Assert.NotNull(data.Department);
            Assert.Equal(dept.Id, data.Department.Id);
            Assert.Equal(dept.Name, data.Department.Name);
        }

        [Fact]
        public async Task Can_Get_Departments()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            _fixture.Context.Departments.Add(dept);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/departments";

            // act
            var response = await _fixture.SendAsync("GET", route, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeserializer>()
                .DeserializeList<DepartmentResource>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_Department_By_Id()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            _fixture.Context.Departments.Add(dept);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/departments/{dept.Id}";

            // act
            var (response, data) = await _fixture.GetAsync<DepartmentResource>(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(dept.Id, data.Id);
            Assert.Equal(dept.Name, data.Name);
        }

        [Fact]
        public async Task Can_Get_Department_With_Relationships()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            _fixture.Context.Departments.Add(dept);

            var course = _fixture.CourseFaker.Generate();
            course.Department = dept;
            _fixture.Context.Courses.Add(course);

            var othercourse = _fixture.CourseFaker.Generate();
            othercourse.Department = dept;
            _fixture.Context.Courses.Add(othercourse);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/departments/{dept.Id}?include=courses";

            // act
            var (response, data) = await _fixture.GetAsync<DepartmentResource>(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);

            Assert.Equal(dept.Id, data.Id);
            Assert.Equal(dept.Name, data.Name);

            Assert.NotEmpty(data.Courses);
            Assert.Equal(2, data.Courses.Count);
        }

        [Fact]
        public async Task Can_Get_Students()
        {
            // arrange
            var student = _fixture.StudentFaker.Generate();
            _fixture.Context.Students.Add(student);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/students";

            // act
            var response = await _fixture.SendAsync("GET", route, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeserializer>()
                .DeserializeList<StudentResource>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_Student_By_Id()
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
            var deserializedBody = (StudentResource)_fixture.Server.GetService<IJsonApiDeserializer>()
                .Deserialize(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.Equal(student.FirstName, deserializedBody.FirstName);
            Assert.Equal(student.LastName, deserializedBody.LastName);
        }

        [Fact]
        public async Task Can_Get_Student_With_Relationships()
        {
            // arrange
            var course = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var student = _fixture.StudentFaker.Generate();
            _fixture.Context.Students.Add(student);
            _fixture.Context.SaveChanges();

            var reg = new CourseStudentEntity(course, student);
            _fixture.Context.Registrations.Add(reg);
            _fixture.Context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/students/{student.Id}?include=courses";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Server.CreateClient().SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = (StudentResource)_fixture.Server.GetService<IJsonApiDeserializer>()
                .Deserialize(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);

            Assert.Equal(student.FirstName, deserializedBody.FirstName);
            Assert.Equal(student.LastName, deserializedBody.LastName);

            Assert.NotEmpty(deserializedBody.Courses);
        }
    }
}
