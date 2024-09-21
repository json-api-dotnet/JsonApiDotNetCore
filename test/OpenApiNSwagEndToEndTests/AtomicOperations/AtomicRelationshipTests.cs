using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenApiNSwagEndToEndTests.AtomicOperations.GeneratedCode;
using OpenApiTests;
using OpenApiTests.AtomicOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.AtomicOperations;

public sealed class AtomicRelationshipTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly OperationsFakers _fakers;

    public AtomicRelationshipTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<OperationsController>();

        testContext.ConfigureServices(services => services.AddSingleton<ISystemClock, FrozenSystemClock>());

        _fakers = new OperationsFakers(testContext.Factory.Services);
    }

    [Fact]
    public async Task Can_update_ToOne_relationship()
    {
        // Arrange
        Enrollment existingEnrollment = _fakers.Enrollment.GenerateOne();
        existingEnrollment.Student = _fakers.Student.GenerateOne();
        existingEnrollment.Course = _fakers.Course.GenerateOne();

        Student existingStudent = _fakers.Student.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingEnrollment, existingStudent);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new UpdateEnrollmentStudentRelationshipOperation
                {
                    Ref = new EnrollmentStudentRelationshipIdentifier
                    {
                        Id = existingEnrollment.StringId!
                    },
                    Data = new StudentIdentifierInRequest
                    {
                        Id = existingStudent.StringId!
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
            Enrollment enrollmentInDatabase = await dbContext.Enrollments.Include(enrollment => enrollment.Student).FirstWithIdAsync(existingEnrollment.Id);

            enrollmentInDatabase.Student.ShouldNotBeNull();
            enrollmentInDatabase.Student.Id.Should().Be(existingStudent.Id);
        });
    }

    [Fact]
    public async Task Can_update_ToMany_relationship()
    {
        // Arrange
        Teacher existingTeacher = _fakers.Teacher.GenerateOne();
        existingTeacher.Teaches = _fakers.Course.GenerateSet(1);
        List<Course> existingCourses = _fakers.Course.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Add(existingTeacher);
            dbContext.AddRange(existingCourses);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new UpdateTeacherTeachesRelationshipOperation
                {
                    Ref = new TeacherTeachesRelationshipIdentifier
                    {
                        Id = existingTeacher.StringId!
                    },
                    Data =
                    [
                        new CourseIdentifierInRequest
                        {
                            Id = existingCourses.ElementAt(0).Id
                        },
                        new CourseIdentifierInRequest
                        {
                            Id = existingCourses.ElementAt(1).Id
                        }
                    ]
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Teacher teacherInDatabase = await dbContext.Teachers.Include(teacher => teacher.Teaches).FirstWithIdAsync(existingTeacher.Id);

            teacherInDatabase.Teaches.ShouldHaveCount(2);
            teacherInDatabase.Teaches.Should().ContainSingle(course => course.Id == existingCourses.ElementAt(0).Id);
            teacherInDatabase.Teaches.Should().ContainSingle(course => course.Id == existingCourses.ElementAt(1).Id);
        });
    }

    [Fact]
    public async Task Can_add_to_ToMany_relationship()
    {
        // Arrange
        Teacher existingTeacher = _fakers.Teacher.GenerateOne();
        existingTeacher.Teaches = _fakers.Course.GenerateSet(1);
        List<Course> existingCourses = _fakers.Course.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Add(existingTeacher);
            dbContext.AddRange(existingCourses);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new AddToTeacherTeachesRelationshipOperation
                {
                    Ref = new TeacherTeachesRelationshipIdentifier
                    {
                        Id = existingTeacher.StringId!
                    },
                    Data =
                    [
                        new CourseIdentifierInRequest
                        {
                            Id = existingCourses.ElementAt(0).Id
                        },
                        new CourseIdentifierInRequest
                        {
                            Id = existingCourses.ElementAt(1).Id
                        }
                    ]
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Teacher teacherInDatabase = await dbContext.Teachers.Include(teacher => teacher.Teaches).FirstWithIdAsync(existingTeacher.Id);

            teacherInDatabase.Teaches.ShouldHaveCount(3);
            teacherInDatabase.Teaches.Should().ContainSingle(course => course.Id == existingTeacher.Teaches.ElementAt(0).Id);
            teacherInDatabase.Teaches.Should().ContainSingle(course => course.Id == existingCourses.ElementAt(0).Id);
            teacherInDatabase.Teaches.Should().ContainSingle(course => course.Id == existingCourses.ElementAt(1).Id);
        });
    }

    [Fact]
    public async Task Can_remove_from_ToMany_relationship()
    {
        // Arrange
        Teacher existingTeacher = _fakers.Teacher.GenerateOne();
        existingTeacher.Teaches = _fakers.Course.GenerateSet(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Add(existingTeacher);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new RemoveFromTeacherTeachesRelationshipOperation
                {
                    Ref = new TeacherTeachesRelationshipIdentifier
                    {
                        Id = existingTeacher.StringId!
                    },
                    Data =
                    [
                        new CourseIdentifierInRequest
                        {
                            Id = existingTeacher.Teaches.ElementAt(0).Id
                        },
                        new CourseIdentifierInRequest
                        {
                            Id = existingTeacher.Teaches.ElementAt(2).Id
                        }
                    ]
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Teacher teacherInDatabase = await dbContext.Teachers.Include(teacher => teacher.Teaches).FirstWithIdAsync(existingTeacher.Id);

            teacherInDatabase.Teaches.ShouldHaveCount(1);
            teacherInDatabase.Teaches.ElementAt(0).Id.Should().Be(existingTeacher.Teaches.ElementAt(1).Id);
        });
    }
}
