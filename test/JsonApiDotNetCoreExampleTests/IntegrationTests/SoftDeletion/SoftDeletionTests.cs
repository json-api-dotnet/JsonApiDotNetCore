using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    public sealed class SoftDeletionTests : IClassFixture<IntegrationTestContext<TestableStartup<SoftDeletionDbContext>, SoftDeletionDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<SoftDeletionDbContext>, SoftDeletionDbContext> _testContext;

        public SoftDeletionTests(IntegrationTestContext<TestableStartup<SoftDeletionDbContext>, SoftDeletionDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped<IResourceDefinition<Company>, SoftDeletionResourceDefinition<Company>>();
                services.AddScoped<IResourceDefinition<Department>, SoftDeletionResourceDefinition<Department>>();
            });
        }

        [Fact]
        public async Task Can_get_primary_resources()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department
                {
                    Name = "Sales",
                    IsSoftDeleted = true
                },
                new Department
                {
                    Name = "Marketing"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Department>();
                dbContext.Departments.AddRange(departments);

                await dbContext.SaveChangesAsync();
            });

            var route = "/departments";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(departments[1].StringId);
        }

        [Fact]
        public async Task Can_filter_in_primary_resources()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department
                {
                    Name = "Support"
                },
                new Department
                {
                    Name = "Sales",
                    IsSoftDeleted = true
                },
                new Department
                {
                    Name = "Marketing"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Department>();
                dbContext.Departments.AddRange(departments);

                await dbContext.SaveChangesAsync();
            });

            var route = "/departments?filter=startsWith(name,'S')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(departments[0].StringId);
        }

        [Fact]
        public async Task Cannot_get_deleted_primary_resource_by_ID()
        {
            // Arrange
            var department = new Department
            {
                Name = "Sales",
                IsSoftDeleted = true
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Departments.Add(department);

                await dbContext.SaveChangesAsync();
            });

            var route = "/departments/" + department.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be($"Resource of type 'departments' with ID '{department.StringId}' does not exist.");
            responseDocument.Errors[0].Source.Parameter.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_secondary_resources()
        {
            // Arrange
            var company = new Company
            {
                Departments = new List<Department>
                {
                    new Department
                    {
                        Name = "Sales",
                        IsSoftDeleted = true
                    },
                    new Department
                    {
                        Name = "Marketing"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Companies.Add(company);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/companies/{company.StringId}/departments";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(company.Departments.Skip(1).Single().StringId);
        }

        [Fact]
        public async Task Cannot_get_secondary_resources_for_deleted_parent()
        {
            // Arrange
            var company = new Company
            {
                IsSoftDeleted = true,
                Departments = new List<Department>
                {
                    new Department
                    {
                        Name = "Marketing"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Companies.Add(company);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/companies/{company.StringId}/departments";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be($"Resource of type 'companies' with ID '{company.StringId}' does not exist.");
            responseDocument.Errors[0].Source.Parameter.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_primary_resources_with_include()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company
                {
                    Name = "Acme Corporation",
                    IsSoftDeleted = true,
                    Departments = new List<Department>
                    {
                        new Department
                        {
                            Name = "Recruitment"
                        }
                    }
                },
                new Company
                {
                    Name = "AdventureWorks",
                    Departments = new List<Department>
                    {
                        new Department
                        {
                            Name = "Reception"
                        },
                        new Department
                        {
                            Name = "Sales",
                            IsSoftDeleted = true
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Company>();
                dbContext.Companies.AddRange(companies);

                await dbContext.SaveChangesAsync();
            });

            var route = "/companies?include=departments";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Type.Should().Be("companies");
            responseDocument.ManyData[0].Id.Should().Be(companies[1].StringId);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("departments");
            responseDocument.Included[0].Id.Should().Be(companies[1].Departments.First().StringId);
        }

        [Fact]
        public async Task Can_get_relationship()
        {
            // Arrange
            var company = new Company
            {
                Departments = new List<Department>
                {
                    new Department
                    {
                        Name = "Sales",
                        IsSoftDeleted = true
                    },
                    new Department
                    {
                        Name = "Marketing"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Companies.Add(company);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/companies/{company.StringId}/relationships/departments";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(company.Departments.Skip(1).Single().StringId);
        }

        [Fact]
        public async Task Cannot_get_relationship_for_deleted_parent()
        {
            // Arrange
            var company = new Company
            {
                IsSoftDeleted = true,
                Departments = new List<Department>
                {
                    new Department
                    {
                        Name = "Marketing"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Companies.Add(company);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/companies/{company.StringId}/relationships/departments";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be($"Resource of type 'companies' with ID '{company.StringId}' does not exist.");
            responseDocument.Errors[0].Source.Parameter.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_update_deleted_resource()
        {
            // Arrange
            var company = new Company
            {
                IsSoftDeleted = true
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Companies.Add(company);

                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "companies",
                    id = company.StringId,
                    attributes = new
                    {
                        name = "Umbrella Corporation"
                    }
                }
            };

            var route = "/companies/" + company.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be($"Resource of type 'companies' with ID '{company.StringId}' does not exist.");
            responseDocument.Errors[0].Source.Parameter.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_update_relationship_for_deleted_parent()
        {
            // Arrange
            var company = new Company
            {
                IsSoftDeleted = true,
                Departments = new List<Department>
                {
                    new Department
                    {
                        Name = "Marketing"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Companies.Add(company);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/companies/{company.StringId}/relationships/departments";

            var requestBody = new
            {
                data = new object[0]
            };

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be($"Resource of type 'companies' with ID '{company.StringId}' does not exist.");
            responseDocument.Errors[0].Source.Parameter.Should().BeNull();
        }
    }
}
