using JsonApiDotNetCoreExample.Models.Resources;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ResourceEntitySeparationExampleTests.Acceptance
{
    [Collection("TestCollection")]
    public class AddTests
    {
        private readonly TestFixture _fixture;

        public AddTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Create_Course()
        {
            // arrange
            var route = $"/api/v1/courses/";
            var course = _fixture.CourseFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "courses",
                    attributes = new Dictionary<string, object>()
                    {
                        { "number", course.Number },
                        { "title", course.Title },
                        { "description", course.Description }
                    }
                }
            };
            
            // act
            var (response, data) = await _fixture.PostAsync<CourseResource>(route, content);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(course.Number, data.Number);
            Assert.Equal(course.Title, data.Title);
            Assert.Equal(course.Description, data.Description);
        }

        [Fact]
        public async Task Can_Create_Department()
        {
            // arrange
            var route = $"/api/v1/departments/";
            var dept = _fixture.DepartmentFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "departments",
                    attributes = new Dictionary<string, string>()
                    {
                        { "name", dept.Name }
                    }
                }
            };

            // act 
            var (response, data) = await _fixture.PostAsync<DepartmentResource>(route, content);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(dept.Name, data.Name);
        }

        [Fact]
        public async Task Can_Create_Student()
        {
            // arrange
            var route = $"/api/v1/students/";
            var student = _fixture.StudentFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "students",
                    attributes = new Dictionary<string, string>()
                    {
                        { "firstname", student.FirstName },
                        { "lastname", student.LastName }
                    }
                }
            };

            // act 
            var (response, data) = await _fixture.PostAsync<StudentResource>(route, content);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(student.FirstName, data.FirstName);
            Assert.Equal(student.LastName, data.LastName);
        }
    }
}
