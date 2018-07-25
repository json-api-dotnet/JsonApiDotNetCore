using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ResourceEntitySeparationExampleTests.Acceptance
{
    [Collection("TestCollection")]
    public class DeleteTests
    {
        private readonly TestFixture _fixture;

        public DeleteTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Delete_Course()
        {
            // arrange
            var course = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/courses/{course.Id}";
            
            // act
            var response = await _fixture.DeleteAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Can_Delete_Department()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            _fixture.Context.Departments.Add(dept);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/departments/{dept.Id}";

            // act
            var response = await _fixture.DeleteAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Can_Delete_Student()
        {
            // arrange
            var student = _fixture.StudentFaker.Generate();
            _fixture.Context.Students.Add(student);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/students/{student.Id}";

            // act
            var response = await _fixture.DeleteAsync(route);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
