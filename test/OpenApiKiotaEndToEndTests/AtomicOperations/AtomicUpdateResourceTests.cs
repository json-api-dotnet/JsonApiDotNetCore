using FluentAssertions;
using JsonApiDotNetCore.Resources;
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

public sealed class AtomicUpdateResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly OperationsFakers _fakers;

    public AtomicUpdateResourceTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<OperationsController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));

        _fakers = new OperationsFakers(testContext.Factory.Services);
    }

    [Fact]
    public async Task Can_update_resource_with_attributes()
    {
        // Arrange
        Student existingStudent = _fakers.Student.GenerateOne();
        string newName = _fakers.Student.GenerateOne().Name;
        string? newEmailAddress = _fakers.Student.GenerateOne().EmailAddress;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Students.Add(existingStudent);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new AtomicOperationsClient(requestAdapter);

        OperationsRequestDocument requestBody = new()
        {
            AtomicOperations =
            [
                new UpdateStudentOperation
                {
                    Op = UpdateOperationCode.Update,
                    Data = new DataInUpdateStudentRequest
                    {
                        Type = StudentResourceType.Students,
                        Id = existingStudent.StringId!,
                        Attributes = new AttributesInUpdateStudentRequest
                        {
                            Name = newName,
                            EmailAddress = newEmailAddress
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
        DataInStudentResponse studentData = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<DataInStudentResponse>().Which;

        studentData.Id.Should().Be(existingStudent.StringId);
        studentData.Attributes.Should().NotBeNull();
        studentData.Attributes.Name.Should().Be(newName);
        studentData.Attributes.EmailAddress.Should().Be(newEmailAddress);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Student studentInDatabase = await dbContext.Students.FirstWithIdAsync(existingStudent.Id);

            studentInDatabase.Name.Should().Be(newName);
            studentInDatabase.EmailAddress.Should().Be(newEmailAddress);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_attributes_using_ref()
    {
        // Arrange
        Student existingStudent = _fakers.Student.GenerateOne();
        string? newEmailAddress = _fakers.Student.GenerateOne().EmailAddress;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Students.Add(existingStudent);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new AtomicOperationsClient(requestAdapter);

        OperationsRequestDocument requestBody = new()
        {
            AtomicOperations =
            [
                new UpdateStudentOperation
                {
                    Op = UpdateOperationCode.Update,
                    Ref = new StudentIdentifierInRequest
                    {
                        Type = ResourceType.Students,
                        Id = existingStudent.StringId!
                    },
                    Data = new DataInUpdateStudentRequest
                    {
                        Type = StudentResourceType.Students,
                        Id = existingStudent.StringId!,
                        Attributes = new AttributesInUpdateStudentRequest
                        {
                            EmailAddress = newEmailAddress
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
        DataInStudentResponse studentData = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<DataInStudentResponse>().Which;

        studentData.Id.Should().Be(existingStudent.StringId);
        studentData.Attributes.Should().NotBeNull();
        studentData.Attributes.Name.Should().Be(existingStudent.Name);
        studentData.Attributes.EmailAddress.Should().Be(newEmailAddress);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Student studentInDatabase = await dbContext.Students.FirstWithIdAsync(existingStudent.Id);

            studentInDatabase.Name.Should().Be(existingStudent.Name);
            studentInDatabase.EmailAddress.Should().Be(newEmailAddress);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_attributes_and_relationships()
    {
        // Arrange
        Enrollment existingEnrollment = _fakers.Enrollment.GenerateOne();
        existingEnrollment.Student = _fakers.Student.GenerateOne();
        existingEnrollment.Course = _fakers.Course.GenerateOne();

        Student existingStudent = _fakers.Student.GenerateOne();
        Course existingCourse = _fakers.Course.GenerateOne();
        DateOnly newEnrolledAt = _fakers.Enrollment.GenerateOne().EnrolledAt;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingEnrollment, existingStudent, existingCourse);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new AtomicOperationsClient(requestAdapter);

        OperationsRequestDocument requestBody = new()
        {
            AtomicOperations =
            [
                new UpdateEnrollmentOperation
                {
                    Op = UpdateOperationCode.Update,
                    Data = new DataInUpdateEnrollmentRequest
                    {
                        Type = EnrollmentResourceType.Enrollments,
                        Id = existingEnrollment.StringId!,
                        Attributes = new AttributesInUpdateEnrollmentRequest
                        {
                            EnrolledAt = newEnrolledAt
                        },
                        Relationships = new RelationshipsInUpdateEnrollmentRequest
                        {
                            Course = new ToOneCourseInRequest
                            {
                                Data = new CourseIdentifierInRequest
                                {
                                    Type = ResourceType.Courses,
                                    Id = existingCourse.Id
                                }
                            },
                            Student = new ToOneStudentInRequest
                            {
                                Data = new StudentIdentifierInRequest
                                {
                                    Type = ResourceType.Students,
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
        DataInEnrollmentResponse enrollmentData = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<DataInEnrollmentResponse>().Which;

        enrollmentData.Id.Should().Be(existingEnrollment.StringId);
        enrollmentData.Attributes.Should().NotBeNull();
        enrollmentData.Attributes.EnrolledAt.Should().Be((Date)newEnrolledAt);
        enrollmentData.Attributes.GraduatedAt.Should().Be((Date)existingEnrollment.GraduatedAt!.Value);
        enrollmentData.Attributes.HasGraduated.Should().Be(existingEnrollment.HasGraduated);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            Enrollment enrollmentInDatabase = await dbContext.Enrollments
                .Include(enrollment => enrollment.Student)
                .Include(enrollment => enrollment.Course)
                .FirstWithIdAsync(existingEnrollment.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            enrollmentInDatabase.EnrolledAt.Should().Be(newEnrolledAt);
            enrollmentInDatabase.GraduatedAt.Should().Be(existingEnrollment.GraduatedAt);
            enrollmentInDatabase.HasGraduated.Should().Be(existingEnrollment.HasGraduated);

            enrollmentInDatabase.Student.Should().NotBeNull();
            enrollmentInDatabase.Student.Id.Should().Be(existingStudent.Id);

            enrollmentInDatabase.Course.Should().NotBeNull();
            enrollmentInDatabase.Course.Id.Should().Be(existingCourse.Id);
        });
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
