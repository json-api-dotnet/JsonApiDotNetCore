using FluentAssertions;
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

public sealed class AtomicCreateResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly OperationsFakers _fakers;

    public AtomicCreateResourceTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<OperationsController>();

        _fakers = new OperationsFakers(testContext.Factory.Services);
    }

    [Fact]
    public async Task Can_create_resource_with_attributes()
    {
        // Arrange
        Teacher newTeacher = _fakers.Teacher.GenerateOne();

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
        OperationsResponseDocument? response = await apiClient.Operations.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();

        response.AtomicResults.Should().HaveCount(1);
        TeacherDataInResponse teacherDataInResponse = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<TeacherDataInResponse>().Which;

        teacherDataInResponse.Attributes.Should().NotBeNull();
        teacherDataInResponse.Attributes.Name.Should().Be(newTeacher.Name);
        teacherDataInResponse.Attributes.EmailAddress.Should().Be(newTeacher.EmailAddress);

        long newTeacherId = long.Parse(teacherDataInResponse.Id!);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new AtomicOperationsClient(requestAdapter);

        OperationsRequestDocument requestBody = new()
        {
            AtomicOperations =
            [
                new CreateEnrollmentOperation
                {
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateEnrollmentRequest
                    {
                        Type = EnrollmentResourceType.Enrollments,
                        Attributes = new AttributesInCreateEnrollmentRequest
                        {
                            EnrolledAt = newEnrollment.EnrolledAt
                        },
                        Relationships = new RelationshipsInCreateEnrollmentRequest
                        {
                            Course = new ToOneCourseInRequest
                            {
                                Data = new CourseIdentifierInRequest
                                {
                                    Type = CourseResourceType.Courses,
                                    Id = existingCourse.Id
                                }
                            },
                            Student = new ToOneStudentInRequest
                            {
                                Data = new StudentIdentifierInRequest
                                {
                                    Type = StudentResourceType.Students,
                                    Id = existingStudent.StringId!
                                }
                            }
                        }
                    }
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await apiClient.Operations.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();

        response.AtomicResults.Should().HaveCount(1);
        EnrollmentDataInResponse enrollmentDataInResponse = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<EnrollmentDataInResponse>().Which;

        enrollmentDataInResponse.Attributes.Should().NotBeNull();
        enrollmentDataInResponse.Attributes.EnrolledAt.Should().Be((Date)newEnrollment.EnrolledAt);
        enrollmentDataInResponse.Attributes.GraduatedAt.Should().BeNull();
        enrollmentDataInResponse.Attributes.HasGraduated.Should().BeFalse();

        long newEnrollmentId = long.Parse(enrollmentDataInResponse.Id!);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new AtomicOperationsClient(requestAdapter);

        OperationsRequestDocument requestBody = new()
        {
            AtomicOperations =
            [
                new CreateCourseOperation
                {
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateCourseRequest
                    {
                        Type = CourseResourceType.Courses,
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
        OperationsResponseDocument? response = await apiClient.Operations.PostAsync(requestBody);

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
        _requestAdapterFactory.Dispose();
    }
}
