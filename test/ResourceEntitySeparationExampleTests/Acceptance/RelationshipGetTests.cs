using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Models.Entities;
using JsonApiDotNetCoreExample.Models.Resources;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ResourceEntitySeparationExampleTests.Acceptance
{
    [Collection("TestCollection")]
    public class RelationshipGetTests
    {
        private readonly TestFixture _fixture;

        public RelationshipGetTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Get_Courses_For_Department()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            var course = _fixture.CourseFaker.Generate();
            course.Department = dept;
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/departments/{dept.Id}/courses";

            // act
            var response = await _fixture.SendAsync("GET", route, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<CourseResource>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_Course_Relationships_For_Department()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            var course = _fixture.CourseFaker.Generate();
            course.Department = dept;
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/departments/{dept.Id}/relationships/courses";

            // act
            var response = await _fixture.SendAsync("GET", route, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<CourseResource>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_Courses_For_Student()
        {
            // arrange
            var course = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course);
            
            var student = _fixture.StudentFaker.Generate();
            _fixture.Context.Students.Add(student);

            var reg = new CourseStudentEntity(course, student);
            _fixture.Context.Registrations.Add(reg);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/students/{student.Id}/courses";

            // act
            var response = await _fixture.SendAsync("GET", route, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<CourseResource>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_Course_Relationships_For_Student()
        {
            // arrange
            var course = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course);
            
            var student = _fixture.StudentFaker.Generate();
            _fixture.Context.Students.Add(student);
            
            var reg = new CourseStudentEntity(course, student);
            _fixture.Context.Registrations.Add(reg);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/students/{student.Id}/relationships/courses";

            // act
            var response = await _fixture.SendAsync("GET", route, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<CourseResource>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_Department_For_Course()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            var course = _fixture.CourseFaker.Generate();
            course.Department = dept;
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/courses/{course.Id}/department";

            // act
            var (response, data) = await _fixture.GetAsync<DepartmentResource>(route);
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(dept.Name, data.Name);
        }

        [Fact]
        public async Task Can_Get_Department_Relationships_For_Course()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            var course = _fixture.CourseFaker.Generate();
            course.Department = dept;
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/courses/{course.Id}/relationships/department";

            // act
            var (response, data) = await _fixture.GetAsync<DepartmentResource>(route);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task Can_Get_Students_For_Course()
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

            var route = $"/api/v1/courses/{course.Id}/students";

            // act
            var response = await _fixture.SendAsync("GET", route, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<StudentResource>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_Student_Relationships_For_Course()
        {
            // arrange
            var course = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course);
            
            var student = _fixture.StudentFaker.Generate();
            _fixture.Context.Students.Add(student);
            
            var reg = new CourseStudentEntity(course, student);
            _fixture.Context.Registrations.Add(reg);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/courses/{course.Id}/relationships/students";

            // act
            var response = await _fixture.SendAsync("GET", route, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.Server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<StudentResource>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }
    }
}
