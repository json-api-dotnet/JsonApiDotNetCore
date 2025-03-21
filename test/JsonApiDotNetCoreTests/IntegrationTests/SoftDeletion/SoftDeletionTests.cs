using System.Net;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

public sealed class SoftDeletionTests : IClassFixture<IntegrationTestContext<TestableStartup<SoftDeletionDbContext>, SoftDeletionDbContext>>
{
    private static readonly DateTimeOffset CurrentTime = 1.January(2005).AsUtc();
    private static readonly DateTimeOffset SoftDeletionTime = 1.January(2001).AsUtc();

    private readonly IntegrationTestContext<TestableStartup<SoftDeletionDbContext>, SoftDeletionDbContext> _testContext;
    private readonly SoftDeletionFakers _fakers = new(CurrentTime);

    public SoftDeletionTests(IntegrationTestContext<TestableStartup<SoftDeletionDbContext>, SoftDeletionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CompaniesController>();
        testContext.UseController<DepartmentsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceService<SoftDeletionAwareResourceService<Company, int>>();
            services.AddResourceService<SoftDeletionAwareResourceService<Department, int>>();
        });

        testContext.PostConfigureServices(services => services.Replace(ServiceDescriptor.Singleton<TimeProvider>(new FrozenTimeProvider(CurrentTime))));

        var timeProvider = (FrozenTimeProvider)testContext.Factory.Services.GetRequiredService<TimeProvider>();
        timeProvider.Reset();
    }

    [Fact]
    public async Task Get_primary_resources_excludes_soft_deleted()
    {
        // Arrange
        List<Department> departments = _fakers.Department.GenerateList(2);
        departments[0].SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Department>();
            dbContext.Departments.AddRange(departments);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(departments[1].StringId);
    }

    [Fact]
    public async Task Filter_on_primary_resources_excludes_soft_deleted()
    {
        // Arrange
        List<Department> departments = _fakers.Department.GenerateList(3);

        departments[0].Name = "Support";

        departments[1].Name = "Sales";
        departments[1].SoftDeletedAt = SoftDeletionTime;

        departments[2].Name = "Marketing";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Department>();
            dbContext.Departments.AddRange(departments);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/departments?filter=startsWith(name,'S')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(departments[0].StringId);
    }

    [Fact]
    public async Task Get_primary_resources_with_include_excludes_soft_deleted()
    {
        // Arrange
        List<Company> companies = _fakers.Company.GenerateList(2);

        companies[0].SoftDeletedAt = SoftDeletionTime;
        companies[0].Departments = _fakers.Department.GenerateList(1);

        companies[1].Departments = _fakers.Department.GenerateList(2);
        companies[1].Departments.ElementAt(1).SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Company>();
            dbContext.Companies.AddRange(companies);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/companies?include=departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("companies");
        responseDocument.Data.ManyValue[0].Id.Should().Be(companies[1].StringId);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Type.Should().Be("departments");
        responseDocument.Included[0].Id.Should().Be(companies[1].Departments.ElementAt(0).StringId);
    }

    [Fact]
    public async Task Cannot_get_soft_deleted_primary_resource_by_ID()
    {
        // Arrange
        Department department = _fakers.Department.GenerateOne();
        department.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/departments/{department.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'departments' with ID '{department.StringId}' does not exist.");
    }

    [Fact]
    public async Task Cannot_get_secondary_resources_for_soft_deleted_parent()
    {
        // Arrange
        Company company = _fakers.Company.GenerateOne();
        company.SoftDeletedAt = SoftDeletionTime;
        company.Departments = _fakers.Department.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(company);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/companies/{company.StringId}/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'companies' with ID '{company.StringId}' does not exist.");
    }

    [Fact]
    public async Task Get_secondary_resources_excludes_soft_deleted()
    {
        // Arrange
        Company company = _fakers.Company.GenerateOne();
        company.Departments = _fakers.Department.GenerateList(2);
        company.Departments.ElementAt(0).SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(company);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/companies/{company.StringId}/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(company.Departments.ElementAt(1).StringId);
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_for_soft_deleted_parent()
    {
        // Arrange
        Department department = _fakers.Department.GenerateOne();
        department.SoftDeletedAt = SoftDeletionTime;
        department.Company = _fakers.Company.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/departments/{department.StringId}/company";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'departments' with ID '{department.StringId}' does not exist.");
    }

    [Fact]
    public async Task Cannot_get_soft_deleted_secondary_resource()
    {
        // Arrange
        Department department = _fakers.Department.GenerateOne();
        department.Company = _fakers.Company.GenerateOne();
        department.Company.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/departments/{department.StringId}/company";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.Value.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_get_ToMany_relationship_for_soft_deleted_parent()
    {
        // Arrange
        Company company = _fakers.Company.GenerateOne();
        company.SoftDeletedAt = SoftDeletionTime;
        company.Departments = _fakers.Department.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(company);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/companies/{company.StringId}/relationships/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'companies' with ID '{company.StringId}' does not exist.");
    }

    [Fact]
    public async Task Get_ToMany_relationship_excludes_soft_deleted()
    {
        // Arrange
        Company company = _fakers.Company.GenerateOne();
        company.Departments = _fakers.Department.GenerateList(2);
        company.Departments.ElementAt(0).SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(company);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/companies/{company.StringId}/relationships/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(company.Departments.ElementAt(1).StringId);
    }

    [Fact]
    public async Task Cannot_get_ToOne_relationship_for_soft_deleted_parent()
    {
        // Arrange
        Department department = _fakers.Department.GenerateOne();
        department.SoftDeletedAt = SoftDeletionTime;
        department.Company = _fakers.Company.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/departments/{department.StringId}/relationships/company";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'departments' with ID '{department.StringId}' does not exist.");
    }

    [Fact]
    public async Task Get_ToOne_relationship_excludes_soft_deleted()
    {
        // Arrange
        Department department = _fakers.Department.GenerateOne();
        department.Company = _fakers.Company.GenerateOne();
        department.Company.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/departments/{department.StringId}/relationships/company";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.Value.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_create_resource_with_ToMany_relationship_to_soft_deleted()
    {
        // Arrange
        Department existingDepartment = _fakers.Department.GenerateOne();
        existingDepartment.SoftDeletedAt = SoftDeletionTime;

        string newCompanyName = _fakers.Company.GenerateOne().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Departments.Add(existingDepartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "companies",
                attributes = new
                {
                    name = newCompanyName
                },
                relationships = new
                {
                    departments = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "departments",
                                id = existingDepartment.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/companies";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");

        error.Detail.Should()
            .Be($"Related resource of type 'departments' with ID '{existingDepartment.StringId}' in relationship 'departments' does not exist.");
    }

    [Fact]
    public async Task Cannot_create_resource_with_ToOne_relationship_to_soft_deleted()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();
        existingCompany.SoftDeletedAt = SoftDeletionTime;

        string newDepartmentName = _fakers.Department.GenerateOne().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(existingCompany);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "departments",
                attributes = new
                {
                    name = newDepartmentName
                },
                relationships = new
                {
                    company = new
                    {
                        data = new
                        {
                            type = "companies",
                            id = existingCompany.StringId
                        }
                    }
                }
            }
        };

        const string route = "/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'companies' with ID '{existingCompany.StringId}' in relationship 'company' does not exist.");
    }

    [Fact]
    public async Task Cannot_update_soft_deleted_resource()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();
        existingCompany.SoftDeletedAt = SoftDeletionTime;

        string newCompanyName = _fakers.Company.GenerateOne().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(existingCompany);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "companies",
                id = existingCompany.StringId,
                attributes = new
                {
                    name = newCompanyName
                }
            }
        };

        string route = $"/companies/{existingCompany.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'companies' with ID '{existingCompany.StringId}' does not exist.");
    }

    [Fact]
    public async Task Cannot_update_resource_with_ToMany_relationship_to_soft_deleted()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();

        Department existingDepartment = _fakers.Department.GenerateOne();
        existingDepartment.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingCompany, existingDepartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "companies",
                id = existingCompany.StringId,
                relationships = new
                {
                    departments = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "departments",
                                id = existingDepartment.StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/companies/{existingCompany.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");

        error.Detail.Should()
            .Be($"Related resource of type 'departments' with ID '{existingDepartment.StringId}' in relationship 'departments' does not exist.");
    }

    [Fact]
    public async Task Cannot_update_resource_with_ToOne_relationship_to_soft_deleted()
    {
        // Arrange
        Department existingDepartment = _fakers.Department.GenerateOne();

        Company existingCompany = _fakers.Company.GenerateOne();
        existingCompany.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingDepartment, existingCompany);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "departments",
                id = existingDepartment.StringId,
                relationships = new
                {
                    company = new
                    {
                        data = new
                        {
                            type = "companies",
                            id = existingCompany.StringId
                        }
                    }
                }
            }
        };

        string route = $"/departments/{existingDepartment.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'companies' with ID '{existingCompany.StringId}' in relationship 'company' does not exist.");
    }

    [Fact]
    public async Task Cannot_update_ToMany_relationship_for_soft_deleted_parent()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();
        existingCompany.SoftDeletedAt = SoftDeletionTime;
        existingCompany.Departments = _fakers.Department.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(existingCompany);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/companies/{existingCompany.StringId}/relationships/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'companies' with ID '{existingCompany.StringId}' does not exist.");
    }

    [Fact]
    public async Task Cannot_update_ToMany_relationship_to_soft_deleted()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();

        Department existingDepartment = _fakers.Department.GenerateOne();
        existingDepartment.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingCompany, existingDepartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "departments",
                    id = existingDepartment.StringId
                }
            }
        };

        string route = $"/companies/{existingCompany.StringId}/relationships/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");

        error.Detail.Should()
            .Be($"Related resource of type 'departments' with ID '{existingDepartment.StringId}' in relationship 'departments' does not exist.");
    }

    [Fact]
    public async Task Cannot_update_ToOne_relationship_for_soft_deleted_parent()
    {
        // Arrange
        Department existingDepartment = _fakers.Department.GenerateOne();
        existingDepartment.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Departments.Add(existingDepartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/departments/{existingDepartment.StringId}/relationships/company";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'departments' with ID '{existingDepartment.StringId}' does not exist.");
    }

    [Fact]
    public async Task Cannot_update_ToOne_relationship_to_soft_deleted()
    {
        // Arrange
        Department existingDepartment = _fakers.Department.GenerateOne();

        Company existingCompany = _fakers.Company.GenerateOne();
        existingCompany.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingDepartment, existingCompany);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "companies",
                id = existingCompany.StringId
            }
        };

        string route = $"/departments/{existingDepartment.StringId}/relationships/company";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'companies' with ID '{existingCompany.StringId}' in relationship 'company' does not exist.");
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship_for_soft_deleted_parent()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();
        existingCompany.SoftDeletedAt = SoftDeletionTime;

        Department existingDepartment = _fakers.Department.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingCompany, existingDepartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "departments",
                    id = existingDepartment.StringId
                }
            }
        };

        string route = $"/companies/{existingCompany.StringId}/relationships/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'companies' with ID '{existingCompany.StringId}' does not exist.");
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship_with_soft_deleted()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();

        Department existingDepartment = _fakers.Department.GenerateOne();
        existingDepartment.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingCompany, existingDepartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "departments",
                    id = existingDepartment.StringId
                }
            }
        };

        string route = $"/companies/{existingCompany.StringId}/relationships/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");

        error.Detail.Should()
            .Be($"Related resource of type 'departments' with ID '{existingDepartment.StringId}' in relationship 'departments' does not exist.");
    }

    [Fact]
    public async Task Cannot_remove_from_ToMany_relationship_for_soft_deleted_parent()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();
        existingCompany.SoftDeletedAt = SoftDeletionTime;
        existingCompany.Departments = _fakers.Department.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(existingCompany);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "departments",
                    id = existingCompany.Departments.ElementAt(0).StringId
                }
            }
        };

        string route = $"/companies/{existingCompany.StringId}/relationships/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'companies' with ID '{existingCompany.StringId}' does not exist.");
    }

    [Fact]
    public async Task Cannot_remove_from_ToMany_relationship_with_soft_deleted()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();
        existingCompany.Departments = _fakers.Department.GenerateList(1);
        existingCompany.Departments.ElementAt(0).SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(existingCompany);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "departments",
                    id = existingCompany.Departments.ElementAt(0).StringId
                }
            }
        };

        string route = $"/companies/{existingCompany.StringId}/relationships/departments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");

        error.Detail.Should().Be(
            $"Related resource of type 'departments' with ID '{existingCompany.Departments.ElementAt(0).StringId}' in relationship 'departments' does not exist.");
    }

    [Fact]
    public async Task Can_soft_delete_resource()
    {
        // Arrange
        Company existingCompany = _fakers.Company.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Companies.Add(existingCompany);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/companies/{existingCompany.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Company companyInDatabase = await dbContext.Companies.IgnoreQueryFilters().FirstWithIdAsync(existingCompany.Id);

            companyInDatabase.Name.Should().Be(existingCompany.Name);
            companyInDatabase.SoftDeletedAt.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Cannot_delete_soft_deleted_resource()
    {
        // Arrange
        Department existingDepartment = _fakers.Department.GenerateOne();
        existingDepartment.SoftDeletedAt = SoftDeletionTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Departments.Add(existingDepartment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/departments/{existingDepartment.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'departments' with ID '{existingDepartment.StringId}' does not exist.");
    }
}
