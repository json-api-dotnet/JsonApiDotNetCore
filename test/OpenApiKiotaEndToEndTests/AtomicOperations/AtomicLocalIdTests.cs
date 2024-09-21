using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode;
using OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.AtomicOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.AtomicOperations;

public sealed class AtomicLocalIdTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly OperationsFakers _fakers;

    public AtomicLocalIdTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<OperationsController>();

        testContext.ConfigureServices(services => services.AddSingleton<ISystemClock, FrozenSystemClock>());

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new AtomicOperationsClient(requestAdapter);

        OperationsRequestDocument requestBody = new()
        {
            AtomicOperations =
            [
                new CreateTeacherOperation
                {
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateTeacherRequest
                    {
                        Type = TeacherResourceType.Teachers,
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
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateCourseRequest
                    {
                        Type = CourseResourceType.Courses,
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
                    Op = AddOperationCode.Add,
                    Ref = new TeacherTeachesRelationshipIdentifier
                    {
                        Type = TeacherResourceType.Teachers,
                        Lid = teacherLocalId,
                        Relationship = TeacherTeachesRelationshipName.Teaches
                    },
                    Data =
                    [
                        new CourseIdentifierInRequest
                        {
                            Type = CourseResourceType.Courses,
                            Id = newCourse.Id
                        }
                    ]
                },
                new CreateStudentOperation
                {
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateStudentRequest
                    {
                        Type = StudentResourceType.Students,
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
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateEnrollmentRequest
                    {
                        Type = EnrollmentResourceType.Enrollments,
                        Lid = enrollmentLocalId,
                        Attributes = new AttributesInCreateEnrollmentRequest
                        {
                            EnrolledAt = newEnrolledAt
                        },
                        Relationships = new RelationshipsInCreateEnrollmentRequest
                        {
                            Course = new ToOneCourseInRequest
                            {
                                Data = new CourseIdentifierInRequest
                                {
                                    Type = CourseResourceType.Courses,
                                    Id = newCourse.Id
                                }
                            },
                            Student = new ToOneStudentInRequest
                            {
                                Data = new StudentIdentifierInRequest
                                {
                                    Type = StudentResourceType.Students,
                                    Lid = studentLocalId
                                }
                            }
                        }
                    }
                },
                new UpdateStudentMentorRelationshipOperation
                {
                    Op = UpdateOperationCode.Update,
                    Ref = new StudentMentorRelationshipIdentifier
                    {
                        Type = StudentResourceType.Students,
                        Lid = studentLocalId,
                        Relationship = StudentMentorRelationshipName.Mentor
                    },
                    Data = new TeacherIdentifierInRequest
                    {
                        Type = TeacherResourceType.Teachers,
                        Lid = teacherLocalId
                    }
                },
                new DeleteTeacherOperation
                {
                    Op = RemoveOperationCode.Remove,
                    Ref = new TeacherIdentifierInRequest
                    {
                        Type = TeacherResourceType.Teachers,
                        Lid = teacherLocalId
                    }
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await apiClient.Operations.PostAsync(requestBody);

        // Assert
        response.ShouldNotBeNull();

        response.AtomicResults.ShouldHaveCount(7);

        TeacherDataInResponse teacherInResponse = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<TeacherDataInResponse>().Which;
        teacherInResponse.Attributes.ShouldNotBeNull();
        teacherInResponse.Attributes.Name.Should().Be(newTeacher.Name);
        teacherInResponse.Attributes.EmailAddress.Should().Be(newTeacher.EmailAddress);
        long newTeacherId = long.Parse(teacherInResponse.Id!);

        response.AtomicResults.ElementAt(1).Data.Should().BeNull();
        response.AtomicResults.ElementAt(2).Data.Should().BeNull();

        StudentDataInResponse studentInResponse = response.AtomicResults.ElementAt(3).Data.Should().BeOfType<StudentDataInResponse>().Which;
        studentInResponse.Attributes.ShouldNotBeNull();
        studentInResponse.Attributes.Name.Should().Be(newStudent.Name);
        studentInResponse.Attributes.EmailAddress.Should().Be(newStudent.EmailAddress);
        long newStudentId = long.Parse(studentInResponse.Id!);

        EnrollmentDataInResponse enrollmentInResponse = response.AtomicResults.ElementAt(4).Data.Should().BeOfType<EnrollmentDataInResponse>().Which;
        enrollmentInResponse.Attributes.ShouldNotBeNull();
        enrollmentInResponse.Attributes.EnrolledAt.Should().Be((Date)newEnrolledAt);
        long newEnrollmentId = long.Parse(enrollmentInResponse.Id!);

        response.AtomicResults.ElementAt(5).Data.Should().BeNull();
        response.AtomicResults.ElementAt(6).Data.Should().BeNull();

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
            enrollmentInDatabase.Course.ShouldNotBeNull();
            enrollmentInDatabase.Course.Id.Should().Be(newCourse.Id);
            enrollmentInDatabase.Student.Id.Should().Be(newStudentId);
        });
    }
}
