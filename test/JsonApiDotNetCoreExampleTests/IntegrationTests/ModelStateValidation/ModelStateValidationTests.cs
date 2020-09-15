using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class ModelStateValidationTests : IClassFixture<IntegrationTestContext<ModelStateValidationStartup<ModelStateDbContext>, ModelStateDbContext>>
    {
        private readonly IntegrationTestContext<ModelStateValidationStartup<ModelStateDbContext>, ModelStateDbContext> _testContext;

        public ModelStateValidationTests(IntegrationTestContext<ModelStateValidationStartup<ModelStateDbContext>, ModelStateDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task When_posting_resource_with_omitted_required_attribute_value_it_must_fail()
        {
            // Arrange
            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    attributes = new Dictionary<string, object>
                    {
                        ["industry"] = "Transport"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The CompanyName field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_posting_resource_with_null_for_required_attribute_value_it_must_fail()
        {
            // Arrange
            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = null,
                        ["industry"] = "Transport"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The CompanyName field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_posting_resource_with_invalid_attribute_value_it_must_fail()
        {
            // Arrange
            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "!@#$%^&*().-"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The field CompanyName must match the regular expression '^[\\w\\s]+$'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_posting_resource_with_valid_attribute_value_it_must_succeed()
        {
            // Arrange
            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "Massive Dynamic"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes["companyName"].Should().Be("Massive Dynamic");
        }

        [Fact]
        public async Task When_patching_resource_with_omitted_required_attribute_value_it_must_succeed()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Massive Dynamic",
                Industry = "Manufacturing"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["industry"] = "Electronics"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }

        [Fact]
        public async Task When_patching_resource_with_null_for_required_attribute_value_it_must_fail()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Massive Dynamic"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = null
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The CompanyName field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_patching_resource_with_invalid_attribute_value_it_must_fail()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Massive Dynamic"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "!@#$%^&*().-"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The field CompanyName must match the regular expression '^[\\w\\s]+$'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_patching_resource_with_valid_attribute_value_it_must_succeed()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Massive Dynamic",
                Industry = "Manufacturing"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "Umbrella Corporation"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }
    }
}
