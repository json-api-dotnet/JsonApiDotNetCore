using JsonApiDotNetCoreExample.Models.Resources;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ResourceEntitySeparationExampleTests.Acceptance
{
    [Collection("TestCollection")]
    public class UpdateTests
    {
        private readonly TestFixture _fixture;

        public UpdateTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Update_Course()
        {
            // arrange
            var course = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var updatedCourse = _fixture.CourseFaker.Generate();

            var route = $"/api/v1/courses/{course.Id}";
            var content = new
            {
                data = new
                {
                    type = "courses",
                    attributes = new Dictionary<string, object>()
                    {
                        { "number", updatedCourse.Number }
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PatchAsync<CourseResource>(route, content);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(course.Id, data.Id);
            Assert.Equal(updatedCourse.Number, data.Number);
        }

        [Fact]
        public async Task Can_Update_Department()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            _fixture.Context.Departments.Add(dept);
            _fixture.Context.SaveChanges();

            var updatedDept = _fixture.DepartmentFaker.Generate();

            var route = $"/api/v1/departments/{dept.Id}";
            var content = new
            {
                data = new
                {
                    type = "departments",
                    attributes = new Dictionary<string, object>()
                    {
                        { "name", updatedDept.Name }
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PatchAsync<DepartmentResource>(route, content);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(dept.Id, data.Id);
            Assert.Equal(updatedDept.Name, data.Name);
        }

        [Fact]
        public async Task Can_Update_Student()
        {
            // arrange
            var student = _fixture.StudentFaker.Generate();
            _fixture.Context.Students.Add(student);
            _fixture.Context.SaveChanges();

            var updatedStudent = _fixture.StudentFaker.Generate();

            var route = $"/api/v1/students/{student.Id}";
            var content = new
            {
                data = new
                {
                    type = "students",
                    attributes = new Dictionary<string, string>()
                    {
                        { "lastname", updatedStudent.LastName }
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PatchAsync<StudentResource>(route, content);
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(student.Id, data.Id);
            Assert.Equal(updatedStudent.LastName, data.LastName);
        }
    }
}
