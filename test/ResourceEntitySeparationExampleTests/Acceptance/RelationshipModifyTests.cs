using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace ResourceEntitySeparationExampleTests.Acceptance
{
    [Collection("TestCollection")]
    public class RelationshipModifyTests
    {
        private readonly TestFixture _fixture;

        public RelationshipModifyTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Patch_HasOne_Relationship()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            _fixture.Context.Departments.Add(dept);

            var course = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/courses/{course.Id}/relationships/department";
            var content = new
            {
                data = new
                {
                    type = "departments",
                    id = $"{dept.Id}"
                }
            };

            // act
            var response = await _fixture.SendAsync("PATCH", route, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _fixture.Context.Entry(course).Reload();
            Assert.Equal(dept.Id, course.DepartmentId);
        }

        [Fact]
        public async Task Can_Patch_HasMany_Relationship()
        {
            // arrange
            var dept = _fixture.DepartmentFaker.Generate();
            _fixture.Context.Departments.Add(dept);

            var course1 = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course1);
            var course2 = _fixture.CourseFaker.Generate();
            _fixture.Context.Courses.Add(course2);
            _fixture.Context.SaveChanges();

            var route = $"/api/v1/departments/{dept.Id}/relationships/courses";
            var content = new
            {
                data = new List<object>
                {
                    new {
                        type = "courses",
                        id = $"{course1.Id}"
                    },
                    new {
                        type = "courses",
                        id = $"{course2.Id}"
                    }
                }
            };

            // act
            var response = await _fixture.SendAsync("PATCH", route, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _fixture.Context.Entry(course1).Reload();
            _fixture.Context.Entry(course2).Reload();
            Assert.Equal(dept.Id, course1.DepartmentId);
            Assert.Equal(dept.Id, course2.DepartmentId);
        }
    }
}
