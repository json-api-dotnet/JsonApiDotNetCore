using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.EntityFrameworkCore;
using OpenApiNSwagEndToEndTests.AtomicOperations.GeneratedCode;
using OpenApiTests;
using OpenApiTests.AtomicOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.AtomicOperations;

public sealed class AtomicLocalIdTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly OperationsFakers _fakers;

    public AtomicLocalIdTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<OperationsController>();

        _fakers = new OperationsFakers(testContext.Factory.Services);
    }

    [Fact]
    public async Task Can_use_local_IDs()
    {
        // Arrange
        Teacher newTeacher = _fakers.Teacher.GenerateOne();
        Course newCourse = _fakers.Course.GenerateOne();
        newCourse.Id = Guid.NewGuid();
        Student newStudent = _fakers.Student.GenerateOne();
        DateOnly newEnrolledAt = _fakers.Enrollment.GenerateOne().EnrolledAt;

        const string teacherLocalId = "teacher-1";
        const string studentLocalId = "student-1";
        const string enrollmentLocalId = "enrollment-1";

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new CreateTeacherOperation
                {
                    Data = new DataInCreateTeacherRequest
                    {
                        Lid = teacherLocalId,
                        Attributes = new AttributesInCreateTeacherRequest
                        {
                            Name = newTeacher.Name,
                            EmailAddress = newTeacher.EmailAddress
                        }
                    }
                },
                new CreateCourseOperation
                {
                    Data = new DataInCreateCourseRequest
                    {
                        Id = newCourse.Id,
                        Attributes = new AttributesInCreateCourseRequest
                        {
                            Subject = newCourse.Subject,
                            Description = newCourse.Description
                        }
                    }
                },
                new AddToTeacherTeachesRelationshipOperation
                {
                    Ref = new TeacherTeachesRelationshipIdentifier
                    {
                        Lid = teacherLocalId
                    },
                    Data =
                    [
                        new CourseIdentifierInRequest
                        {
                            Id = newCourse.Id
                        }
                    ]
                },
                new CreateStudentOperation
                {
                    Data = new DataInCreateStudentRequest
                    {
                        Lid = studentLocalId,
                        Attributes = new AttributesInCreateStudentRequest
                        {
                            Name = newStudent.Name,
                            EmailAddress = newStudent.EmailAddress
                        }
                    }
                },
                new CreateEnrollmentOperation
                {
                    Data = new DataInCreateEnrollmentRequest
                    {
                        Lid = enrollmentLocalId,
                        Attributes = new AttributesInCreateEnrollmentRequest
                        {
                            EnrolledAt = newEnrolledAt.ToDateTime(TimeOnly.MinValue)
                        },
                        Relationships = new RelationshipsInCreateEnrollmentRequest
                        {
                            Course = new ToOneCourseInRequest
                            {
                                Data = new CourseIdentifierInRequest
                                {
                                    Id = newCourse.Id
                                }
                            },
                            Student = new ToOneStudentInRequest
                            {
                                Data = new StudentIdentifierInRequest
                                {
                                    Lid = studentLocalId
                                }
                            }
                        }
                    }
                },
                new UpdateStudentMentorRelationshipOperation
                {
                    Ref = new StudentMentorRelationshipIdentifier
                    {
                        Lid = studentLocalId
                    },
                    Data = new TeacherIdentifierInRequest
                    {
                        Lid = teacherLocalId
                    }
                },
                new DeleteTeacherOperation
                {
                    Ref = new TeacherIdentifierInRequest
                    {
                        Lid = teacherLocalId
                    }
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.Should().NotBeNull();

        response.Atomic_results.Should().HaveCount(7);

        TeacherDataInResponse teacherInResponse = response.Atomic_results.ElementAt(0).Data.Should().BeOfType<TeacherDataInResponse>().Which;
        teacherInResponse.Attributes.Should().NotBeNull();
        teacherInResponse.Attributes.Name.Should().Be(newTeacher.Name);
        teacherInResponse.Attributes.EmailAddress.Should().Be(newTeacher.EmailAddress);
        long newTeacherId = long.Parse(teacherInResponse.Id);

        response.Atomic_results.ElementAt(1).Data.Should().BeNull();
        response.Atomic_results.ElementAt(2).Data.Should().BeNull();

        StudentDataInResponse studentInResponse = response.Atomic_results.ElementAt(3).Data.Should().BeOfType<StudentDataInResponse>().Which;
        studentInResponse.Attributes.Should().NotBeNull();
        studentInResponse.Attributes.Name.Should().Be(newStudent.Name);
        studentInResponse.Attributes.EmailAddress.Should().Be(newStudent.EmailAddress);
        long newStudentId = long.Parse(studentInResponse.Id);

        EnrollmentDataInResponse enrollmentInResponse = response.Atomic_results.ElementAt(4).Data.Should().BeOfType<EnrollmentDataInResponse>().Which;
        enrollmentInResponse.Attributes.Should().NotBeNull();
        enrollmentInResponse.Attributes.EnrolledAt.Should().Be(newEnrolledAt.ToDateTime(TimeOnly.MinValue));
        long newEnrollmentId = long.Parse(enrollmentInResponse.Id);

        response.Atomic_results.ElementAt(5).Data.Should().BeNull();
        response.Atomic_results.ElementAt(6).Data.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Teacher? teacherInDatabase = await dbContext.Teachers.FirstWithIdOrDefaultAsync(newTeacherId);
            teacherInDatabase.Should().BeNull();

            Course courseInDatabase = await dbContext.Courses.Include(course => course.TaughtBy).FirstWithIdAsync(newCourse.Id);
            courseInDatabase.Subject.Should().Be(newCourse.Subject);
            courseInDatabase.Description.Should().Be(newCourse.Description);
            courseInDatabase.TaughtBy.Should().BeEmpty();

            Student studentInDatabase = await dbContext.Students.Include(student => student.Mentor).FirstWithIdAsync(newStudentId);
            studentInDatabase.Name.Should().Be(newStudent.Name);
            studentInDatabase.EmailAddress.Should().Be(newStudent.EmailAddress);
            studentInDatabase.Mentor.Should().BeNull();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            Enrollment enrollmentInDatabase = await dbContext.Enrollments
                .Include(enrollment => enrollment.Course)
                .Include(enrollment => enrollment.Student)
                .FirstWithIdAsync(newEnrollmentId);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            enrollmentInDatabase.EnrolledAt.Should().Be(newEnrolledAt);
            enrollmentInDatabase.Course.Should().NotBeNull();
            enrollmentInDatabase.Course.Id.Should().Be(newCourse.Id);
            enrollmentInDatabase.Student.Id.Should().Be(newStudentId);
        });
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
