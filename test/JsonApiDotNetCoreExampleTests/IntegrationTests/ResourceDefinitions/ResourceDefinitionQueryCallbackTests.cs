using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions
{
    public sealed class ResourceDefinitionQueryCallbackTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<CallableDbContext>, CallableDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<CallableDbContext>, CallableDbContext> _testContext;

        public ResourceDefinitionQueryCallbackTests(ExampleIntegrationTestContext<TestableStartup<CallableDbContext>, CallableDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped<IResourceDefinition<CallableResource>, CallableResourceDefinition>();
                services.AddSingleton<IUserRolesService, FakeUserRolesService>();
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
        }

        [Fact]
        public async Task Include_from_resource_definition_has_blocked_capability()
        {
            // Arrange
            var userRolesService = (FakeUserRolesService)_testContext.Factory.Services.GetRequiredService<IUserRolesService>();
            userRolesService.AllowIncludeOwner = false;

            var resource = new CallableResource
            {
                Label = "A",
                IsDeleted = false
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<CallableResource>();
                dbContext.CallableResources.Add(resource);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/callableResources?include=owner";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Including owner is not permitted.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Filter_from_resource_definition_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    IsDeleted = true
                },
                new CallableResource
                {
                    Label = "A",
                    IsDeleted = false
                },
                new CallableResource
                {
                    Label = "B",
                    IsDeleted = true
                },
                new CallableResource
                {
                    Label = "B",
                    IsDeleted = false
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<CallableResource>();
                dbContext.CallableResources.AddRange(resources);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/callableResources";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(resources[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(resources[3].StringId);

            responseDocument.Meta["totalResources"].Should().Be(2);
        }

        [Fact]
        public async Task Filter_from_resource_definition_and_query_string_are_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    IsDeleted = true
                },
                new CallableResource
                {
                    Label = "A",
                    IsDeleted = false
                },
                new CallableResource
                {
                    Label = "B",
                    IsDeleted = true
                },
                new CallableResource
                {
                    Label = "B",
                    IsDeleted = false
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<CallableResource>();
                dbContext.CallableResources.AddRange(resources);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/callableResources?filter=equals(label,'B')";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(resources[3].StringId);

            responseDocument.Meta["totalResources"].Should().Be(1);
        }

        [Fact]
        public async Task Sort_from_resource_definition_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    CreatedAt = 1.January(2001),
                    ModifiedAt = 15.January(2001)
                },
                new CallableResource
                {
                    Label = "A",
                    CreatedAt = 1.January(2001),
                    ModifiedAt = 15.December(2001)
                },
                new CallableResource
                {
                    Label = "B",
                    CreatedAt = 1.February(2001),
                    ModifiedAt = 15.January(2001)
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<CallableResource>();
                dbContext.CallableResources.AddRange(resources);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/callableResources";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(resources[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(resources[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(resources[2].StringId);
        }

        [Fact]
        public async Task Sort_from_query_string_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    CreatedAt = 1.January(2001),
                    ModifiedAt = 15.January(2001)
                },
                new CallableResource
                {
                    Label = "A",
                    CreatedAt = 1.January(2001),
                    ModifiedAt = 15.December(2001)
                },
                new CallableResource
                {
                    Label = "B",
                    CreatedAt = 1.February(2001),
                    ModifiedAt = 15.January(2001)
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<CallableResource>();
                dbContext.CallableResources.AddRange(resources);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/callableResources?sort=-createdAt,modifiedAt";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(resources[2].StringId);
            responseDocument.ManyData[1].Id.Should().Be(resources[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(resources[1].StringId);
        }

        [Fact]
        public async Task Page_size_from_resource_definition_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>();

            for (int index = 0; index < 10; index++)
            {
                resources.Add(new CallableResource());
            }

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<CallableResource>();
                dbContext.CallableResources.AddRange(resources);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/callableResources?page[size]=8";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(5);
        }

        [Fact]
        public async Task Attribute_inclusion_from_resource_definition_is_applied_for_empty_query_string()
        {
            // Arrange
            var resource = new CallableResource
            {
                Label = "X",
                PercentageComplete = 5
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.CallableResources.Add(resource);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/callableResources/{resource.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(resource.StringId);
            responseDocument.SingleData.Attributes["label"].Should().Be(resource.Label);
            responseDocument.SingleData.Attributes["percentageComplete"].Should().Be(resource.PercentageComplete);
        }

        [Fact]
        public async Task Attribute_inclusion_from_resource_definition_is_applied_for_non_empty_query_string()
        {
            // Arrange
            var resource = new CallableResource
            {
                Label = "X",
                PercentageComplete = 5
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.CallableResources.Add(resource);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/callableResources/{resource.StringId}?fields[callableResources]=label,status";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(resource.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(2);
            responseDocument.SingleData.Attributes["label"].Should().Be(resource.Label);
            responseDocument.SingleData.Attributes["status"].Should().Be("5% completed.");
            responseDocument.SingleData.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Attribute_exclusion_from_resource_definition_is_applied_for_empty_query_string()
        {
            // Arrange
            var resource = new CallableResource
            {
                Label = "X",
                RiskLevel = 3
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.CallableResources.Add(resource);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/callableResources/{resource.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(resource.StringId);
            responseDocument.SingleData.Attributes["label"].Should().Be(resource.Label);
            responseDocument.SingleData.Attributes.Should().NotContainKey("riskLevel");
        }

        [Fact]
        public async Task Attribute_exclusion_from_resource_definition_is_applied_for_non_empty_query_string()
        {
            // Arrange
            var resource = new CallableResource
            {
                Label = "X",
                RiskLevel = 3
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.CallableResources.Add(resource);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/callableResources/{resource.StringId}?fields[callableResources]=label,riskLevel";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(resource.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["label"].Should().Be(resource.Label);
            responseDocument.SingleData.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Queryable_parameter_handler_from_resource_definition_is_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    RiskLevel = 3
                },
                new CallableResource
                {
                    Label = "A",
                    RiskLevel = 8
                },
                new CallableResource
                {
                    Label = "B",
                    RiskLevel = 3
                },
                new CallableResource
                {
                    Label = "B",
                    RiskLevel = 8
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<CallableResource>();
                dbContext.CallableResources.AddRange(resources);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/callableResources?isHighRisk=true";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(resources[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(resources[3].StringId);
        }

        [Fact]
        public async Task Queryable_parameter_handler_from_resource_definition_and_query_string_filter_are_applied()
        {
            // Arrange
            var resources = new List<CallableResource>
            {
                new CallableResource
                {
                    Label = "A",
                    RiskLevel = 3
                },
                new CallableResource
                {
                    Label = "A",
                    RiskLevel = 8
                },
                new CallableResource
                {
                    Label = "B",
                    RiskLevel = 3
                },
                new CallableResource
                {
                    Label = "B",
                    RiskLevel = 8
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<CallableResource>();
                dbContext.CallableResources.AddRange(resources);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/callableResources?isHighRisk=false&filter=equals(label,'B')";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(resources[2].StringId);
        }

        [Fact]
        public async Task Queryable_parameter_handler_from_resource_definition_is_not_applied_on_secondary_request()
        {
            // Arrange
            var resource = new CallableResource
            {
                RiskLevel = 3,
                Children = new List<CallableResource>
                {
                    new CallableResource
                    {
                        RiskLevel = 3
                    },
                    new CallableResource
                    {
                        RiskLevel = 8
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.CallableResources.Add(resource);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/callableResources/{resource.StringId}/children?isHighRisk=true";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Custom query string parameters cannot be used on nested resource endpoints.");
            error.Detail.Should().Be("Query string parameter 'isHighRisk' cannot be used on a nested resource endpoint.");
            error.Source.Parameter.Should().Be("isHighRisk");
        }

        private sealed class FakeUserRolesService : IUserRolesService
        {
            public bool AllowIncludeOwner { get; set; } = true;
        }
    }
}
