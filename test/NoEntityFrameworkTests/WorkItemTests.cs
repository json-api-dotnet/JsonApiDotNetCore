using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NoEntityFrameworkExample;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using TestBuildingBlocks;
using Xunit;

namespace NoEntityFrameworkTests
{
    public sealed class WorkItemTests : IntegrationTest, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        protected override JsonSerializerOptions SerializerOptions
        {
            get
            {
                var options = _factory.Services.GetRequiredService<IJsonApiOptions>();
                return options.SerializerOptions;
            }
        }

        public WorkItemTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Can_get_WorkItems()
        {
            var workItem = new WorkItem
            {
                Title = "Write some code."
            };

            // Arrange
            await RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/api/v1/workItems";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task Can_get_WorkItem_by_ID()
        {
            // Arrange
            var workItem = new WorkItem
            {
                Title = "Write some code."
            };

            await RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/v1/workItems/{workItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(workItem.StringId);
        }

        [Fact]
        public async Task Can_create_WorkItem()
        {
            // Arrange
            var newWorkItem = new WorkItem
            {
                IsBlocked = true,
                Title = "Some",
                DurationInHours = 2,
                ProjectId = Guid.NewGuid()
            };

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        isBlocked = newWorkItem.IsBlocked,
                        title = newWorkItem.Title,
                        durationInHours = newWorkItem.DurationInHours,
                        projectId = newWorkItem.ProjectId
                    }
                }
            };

            const string route = "/api/v1/workItems/";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Attributes.ShouldNotBeEmpty();
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("isBlocked").With(value => value.Should().Be(newWorkItem.IsBlocked));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newWorkItem.Title));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("durationInHours").With(value => value.Should().Be(newWorkItem.DurationInHours));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("projectId").With(value => value.Should().Be(newWorkItem.ProjectId));
        }

        [Fact]
        public async Task Can_delete_WorkItem()
        {
            // Arrange
            var workItem = new WorkItem
            {
                Title = "Write some code."
            };

            await RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/v1/workItems/{workItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        protected override HttpClient CreateClient()
        {
            return _factory.CreateClient();
        }

        private async Task RunOnDatabaseAsync(Func<AppDbContext, Task> asyncAction)
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await asyncAction(dbContext);
        }
    }
}
