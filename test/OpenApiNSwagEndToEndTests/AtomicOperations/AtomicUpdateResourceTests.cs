using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenApiNSwagEndToEndTests.AtomicOperations.GeneratedCode;
using OpenApiTests;
using OpenApiTests.AtomicOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.AtomicOperations;

public sealed class AtomicUpdateResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly OperationsFakers _fakers;

    public AtomicUpdateResourceTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

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

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new UpdateStudentOperation
                {
                    Data = new DataInUpdateStudentRequest
                    {
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
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.ShouldNotBeNull();

        response.Atomic_results.ShouldHaveCount(1);
        StudentDataInResponse studentDataInResponse = response.Atomic_results.ElementAt(0).Data.Should().BeOfType<StudentDataInResponse>().Which;

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

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new UpdateStudentOperation
                {
                    Ref = new StudentIdentifierInRequest
                    {
                        Id = existingStudent.StringId!
                    },
                    Data = new DataInUpdateStudentRequest
                    {
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
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.ShouldNotBeNull();

        response.Atomic_results.ShouldHaveCount(1);
        StudentDataInResponse studentDataInResponse = response.Atomic_results.ElementAt(0).Data.Should().BeOfType<StudentDataInResponse>().Which;

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

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new UpdateEnrollmentOperation
                {
                    Data = new DataInUpdateEnrollmentRequest
                    {
                        Id = existingEnrollment.StringId!,
                        Attributes = new AttributesInUpdateEnrollmentRequest
                        {
                            EnrolledAt = newEnrolledAt.ToDateTime(TimeOnly.MinValue)
                        },
                        Relationships = new RelationshipsInUpdateEnrollmentRequest
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

        enrollmentDataInResponse.Id.Should().Be(existingEnrollment.StringId);
        enrollmentDataInResponse.Attributes.ShouldNotBeNull();
        enrollmentDataInResponse.Attributes.EnrolledAt.Should().Be(newEnrolledAt.ToDateTime(TimeOnly.MinValue));
        enrollmentDataInResponse.Attributes.GraduatedAt.Should().Be(existingEnrollment.GraduatedAt!.Value.ToDateTime(TimeOnly.MinValue));
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
