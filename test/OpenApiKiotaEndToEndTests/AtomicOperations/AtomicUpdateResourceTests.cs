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

public sealed class AtomicUpdateResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>
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

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<ISystemClock, FrozenSystemClock>();
            services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
        });

        _fakers = new OperationsFakers(testContext.Factory.Services);
    }

    [Fact]
    public async Task Can_update_resource_with_attributes()
    {
        // Arrange
        Student existingStudent = _fakers.Student.Generate();
        string newName = _fakers.Student.Generate().Name;
        string? newEmailAddress = _fakers.Student.Generate().EmailAddress;

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
        response.ShouldNotBeNull();

        response.AtomicResults.ShouldHaveCount(1);
        StudentDataInResponse studentDataInResponse = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<StudentDataInResponse>().Which;

        studentDataInResponse.Id.Should().Be(existingStudent.StringId);
        studentDataInResponse.Attributes.ShouldNotBeNull();
        studentDataInResponse.Attributes.Name.Should().Be(newName);
        studentDataInResponse.Attributes.EmailAddress.Should().Be(newEmailAddress);

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
        Student existingStudent = _fakers.Student.Generate();
        string? newEmailAddress = _fakers.Student.Generate().EmailAddress;

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
                        Type = StudentResourceType.Students,
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
        response.ShouldNotBeNull();

        response.AtomicResults.ShouldHaveCount(1);
        StudentDataInResponse studentDataInResponse = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<StudentDataInResponse>().Which;

        studentDataInResponse.Id.Should().Be(existingStudent.StringId);
        studentDataInResponse.Attributes.ShouldNotBeNull();
        studentDataInResponse.Attributes.Name.Should().Be(existingStudent.Name);
        studentDataInResponse.Attributes.EmailAddress.Should().Be(newEmailAddress);

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
        Enrollment existingEnrollment = _fakers.Enrollment.Generate();
        existingEnrollment.Student = _fakers.Student.Generate();
        existingEnrollment.Course = _fakers.Course.Generate();

        Student existingStudent = _fakers.Student.Generate();
        Course existingCourse = _fakers.Course.Generate();
        DateOnly newEnrolledAt = _fakers.Enrollment.Generate().EnrolledAt;

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
        response.ShouldNotBeNull();

        response.AtomicResults.ShouldHaveCount(1);
        EnrollmentDataInResponse enrollmentDataInResponse = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<EnrollmentDataInResponse>().Which;

        enrollmentDataInResponse.Id.Should().Be(existingEnrollment.StringId);
        enrollmentDataInResponse.Attributes.ShouldNotBeNull();
        enrollmentDataInResponse.Attributes.EnrolledAt.Should().Be((Date)newEnrolledAt);
        enrollmentDataInResponse.Attributes.GraduatedAt.Should().Be((Date)existingEnrollment.GraduatedAt!.Value);
        enrollmentDataInResponse.Attributes.HasGraduated.Should().Be(existingEnrollment.HasGraduated);

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

            enrollmentInDatabase.Student.ShouldNotBeNull();
            enrollmentInDatabase.Student.Id.Should().Be(existingStudent.Id);

            enrollmentInDatabase.Course.ShouldNotBeNull();
            enrollmentInDatabase.Course.Id.Should().Be(existingCourse.Id);
        });
    }
}
