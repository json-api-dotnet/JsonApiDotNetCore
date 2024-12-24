using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using OpenApiNSwagEndToEndTests.AtomicOperations.GeneratedCode;
using OpenApiTests;
using OpenApiTests.AtomicOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;
using CreateTeacherOperation = OpenApiNSwagEndToEndTests.AtomicOperations.GeneratedCode.CreateTeacherOperation;

namespace OpenApiNSwagEndToEndTests.AtomicOperations;

public sealed class AtomicCreateResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly OperationsFakers _fakers;

    public AtomicCreateResourceTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<OperationsController>();

        _fakers = new OperationsFakers(testContext.Factory.Services);
    }

    [Fact]
    public async Task Can_create_resource_with_attributes()
    {
        // Arrange
        Teacher newTeacher = _fakers.Teacher.GenerateOne();

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
                        Attributes = new AttributesInCreateTeacherRequest
                        {
                            Name = newTeacher.Name,
                            EmailAddress = newTeacher.EmailAddress
                        }
                    }
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.ShouldNotBeNull();

        response.Atomic_results.ShouldHaveCount(1);
        TeacherDataInResponse teacherDataInResponse = response.Atomic_results.ElementAt(0).Data.Should().BeOfType<TeacherDataInResponse>().Which;

        teacherDataInResponse.Attributes.ShouldNotBeNull();
        teacherDataInResponse.Attributes.Name.Should().Be(newTeacher.Name);
        teacherDataInResponse.Attributes.EmailAddress.Should().Be(newTeacher.EmailAddress);

        long newTeacherId = long.Parse(teacherDataInResponse.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Teacher teacherInDatabase = await dbContext.Teachers.FirstWithIdAsync(newTeacherId);

            teacherInDatabase.Name.Should().Be(newTeacher.Name);
            teacherInDatabase.EmailAddress.Should().Be(newTeacher.EmailAddress);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_attributes_and_relationships()
    {
        // Arrange
        Student existingStudent = _fakers.Student.GenerateOne();
        Course existingCourse = _fakers.Course.GenerateOne();
        Enrollment newEnrollment = _fakers.Enrollment.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingStudent, existingCourse);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new CreateEnrollmentOperation
                {
                    Data = new DataInCreateEnrollmentRequest
                    {
                        Attributes = new AttributesInCreateEnrollmentRequest
                        {
                            EnrolledAt = newEnrollment.EnrolledAt.ToDateTime(TimeOnly.MinValue)
                        },
                        Relationships = new RelationshipsInCreateEnrollmentRequest
                        {
                            Course = new ToOneCourseInRequest
                            {
                                Data = new CourseIdentifierInRequest
                                {
                                    Id = existingCourse.Id
                                }
                            },
                            Student = new ToOneStudentInRequest
                            {
                                Data = new StudentIdentifierInRequest
                                {
                                    Id = existingStudent.StringId!
                                }
                            }
                        }
                    }
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.ShouldNotBeNull();

        response.Atomic_results.ShouldHaveCount(1);
        EnrollmentDataInResponse enrollmentDataInResponse = response.Atomic_results.ElementAt(0).Data.Should().BeOfType<EnrollmentDataInResponse>().Which;

        enrollmentDataInResponse.Attributes.ShouldNotBeNull();
        enrollmentDataInResponse.Attributes.EnrolledAt.Should().Be(newEnrollment.EnrolledAt.ToDateTime(TimeOnly.MinValue));
        enrollmentDataInResponse.Attributes.GraduatedAt.Should().BeNull();
        enrollmentDataInResponse.Attributes.HasGraduated.Should().BeFalse();

        long newEnrollmentId = long.Parse(enrollmentDataInResponse.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Enrollment enrollmentInDatabase = await dbContext.Enrollments.FirstWithIdAsync(newEnrollmentId);

            enrollmentInDatabase.EnrolledAt.Should().Be(newEnrollment.EnrolledAt);
            enrollmentInDatabase.GraduatedAt.Should().BeNull();
            enrollmentInDatabase.HasGraduated.Should().BeFalse();
        });
    }

    [Fact]
    public async Task Can_create_resource_with_client_generated_ID()
    {
        // Arrange
        Course newCourse = _fakers.Course.GenerateOne();
        newCourse.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new CreateCourseOperation
                {
                    Data = new DataInCreateCourseRequest
                    {
                        Id = newCourse.Id,
                        Attributes = new AttributesInCreateCourseRequest
                        {
                            Subject = newCourse.Subject
                        }
                    }
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Course courseInDatabase = await dbContext.Courses.FirstWithIdAsync(newCourse.Id);

            courseInDatabase.Subject.Should().Be(newCourse.Subject);
            courseInDatabase.Description.Should().BeNull();
        });
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
