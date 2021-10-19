#nullable disable

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.Startups;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.RequestBody
{
    public sealed class WorkflowTests : IClassFixture<IntegrationTestContext<ModelStateValidationStartup<WorkflowDbContext>, WorkflowDbContext>>
    {
        private readonly IntegrationTestContext<ModelStateValidationStartup<WorkflowDbContext>, WorkflowDbContext> _testContext;

        public WorkflowTests(IntegrationTestContext<ModelStateValidationStartup<WorkflowDbContext>, WorkflowDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<WorkflowsController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<WorkflowDefinition>();
            });
        }

        [Fact]
        public async Task Can_create_in_valid_stage()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workflows",
                    attributes = new
                    {
                        stage = WorkflowStage.Created
                    }
                }
            };

            const string route = "/workflows";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
        }

        [Fact]
        public async Task Cannot_create_in_invalid_stage()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workflows",
                    attributes = new
                    {
                        stage = WorkflowStage.Canceled
                    }
                }
            };

            const string route = "/workflows";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Invalid workflow stage.");
            error.Detail.Should().Be("Initial stage of workflow must be 'Created'.");
            error.Source.Pointer.Should().Be("/data/attributes/stage");
        }

        [Fact]
        public async Task Cannot_transition_to_invalid_stage()
        {
            // Arrange
            var existingWorkflow = new Workflow
            {
                Stage = WorkflowStage.OnHold
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Workflows.Add(existingWorkflow);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workflows",
                    id = existingWorkflow.StringId,
                    attributes = new
                    {
                        stage = WorkflowStage.Succeeded
                    }
                }
            };

            string route = $"/workflows/{existingWorkflow.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Invalid workflow stage.");
            error.Detail.Should().Be("Cannot transition from 'OnHold' to 'Succeeded'.");
            error.Source.Pointer.Should().Be("/data/attributes/stage");
        }

        [Fact]
        public async Task Can_transition_to_valid_stage()
        {
            // Arrange
            var existingWorkflow = new Workflow
            {
                Stage = WorkflowStage.InProgress
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Workflows.Add(existingWorkflow);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workflows",
                    id = existingWorkflow.StringId,
                    attributes = new
                    {
                        stage = WorkflowStage.Failed
                    }
                }
            };

            string route = $"/workflows/{existingWorkflow.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }
    }
}
