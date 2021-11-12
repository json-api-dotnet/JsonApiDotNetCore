using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CosmosDbExample.Models;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace CosmosDbTests
{
    public sealed class TodoItemTests : IntegrationTest, IClassFixture<CosmosDbFixture>
    {
        private readonly CosmosDbFixture _fixture;

        protected override JsonSerializerOptions SerializerOptions
        {
            get
            {
                var options = _fixture.Services.GetRequiredService<IJsonApiOptions>();
                return options.SerializerOptions;
            }
        }

        public TodoItemTests(CosmosDbFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// GetAsync(CancellationToken cancellationToken)
        /// </summary>
        [Theory]
        [InlineData("owner")]
        [InlineData("assignee")]
        public async Task Can_get_primary_resources_including_secondary_resource(string relationshipName)
        {
            // Arrange
            string route = $"/api/v1/todoItems?include={relationshipName}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.Should().NotBeNull().And.NotBeEmpty();
            responseDocument.Included.Should().NotBeNull().And.NotBeEmpty();
        }

        /// <summary>
        /// GetAsync(TId id, CancellationToken cancellationToken)
        /// </summary>
        [Theory]
        [InlineData("owner")]
        [InlineData("assignee")]
        public async Task Can_get_primary_resource_by_id_including_secondary_resource(string relationshipName)
        {
            // Arrange
            string route = $"/api/v1/todoItems/{_fixture.TodoItemWithOwnerAndAssigneeId}?include={relationshipName}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();

            responseDocument.Included.Should().NotBeNull().And.NotBeEmpty();
        }

        /// <summary>
        /// GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        /// </summary>
        [Theory]
        [InlineData("owner")]
        [InlineData("assignee")]
        public async Task Can_get_secondary_resource_related_to_primary_resource(string relationshipName)
        {
            // Arrange
            string route = $"/api/v1/todoItems/{_fixture.TodoItemWithOwnerAndAssigneeId}/{relationshipName}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();

            responseDocument.Included.Should().BeNull();
        }

        /// <summary>
        /// GetRelationshipAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        /// </summary>
        [Theory]
        [InlineData("owner")]
        [InlineData("assignee")]
        public async Task Can_get_relationships_of_primary_resource(string relationshipName)
        {
            // Arrange
            string route = $"/api/v1/todoItems/{_fixture.TodoItemWithOwnerAndAssigneeId}/relationships/{relationshipName}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Type.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Id.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Attributes.Should().BeNull();
            responseDocument.Data.SingleValue.Relationships.Should().BeNull();

            responseDocument.Included.Should().BeNull();
        }

        /// <summary>
        /// CreateAsync(TResource resource, CancellationToken cancellationToken)
        /// </summary>
        [Fact]
        public async Task Can_create_primary_resource()
        {
            // Arrange
            const string route = "/api/v1/todoItems";

            var todoItem = new TodoItem
            {
                Description = "To-do created by API",
                Tags = new HashSet<Tag>
                {
                    new()
                    {
                        Name = "First"
                    },
                    new()
                    {
                        Name = "Second"
                    }
                },
                OwnerId = _fixture.BonnieId
            };

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    attributes = new
                    {
                        description = todoItem.Description,
                        tags = todoItem.Tags.Select(tag => new
                        {
                            name = tag.Name
                        })
                    },
                    relationships = new
                    {
                        owner = new
                        {
                            data = new
                            {
                                type = "people",
                                id = todoItem.OwnerId.ToString()
                            }
                        }
                    }
                }
            };

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(todoItem.Description));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("tags").As<HashSet<Tag>>().Should().BeEquivalentTo(todoItem.Tags);
        }

        /// <summary>
        /// SetRelationshipAsync(TId leftId, string relationshipName, object? rightValue, CancellationToken cancellationToken)
        /// </summary>
        [Fact]
        public async Task Can_set_relationship_of_primary_resource()
        {
            // Arrange
            Guid todoItemId = _fixture.TodoItemOwnedByBonnieId;
            Guid assigneeId = _fixture.ClydeId;

            string route = $"/api/v1/todoItems/{todoItemId}/relationships/assignee";

            var requestBody = new
            {
                data = new
                {
                    type = "people",
                    id = assigneeId.ToString()
                }
            };

            // Act
            (HttpResponseMessage httpResponse, Document _) = await ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _fixture.RunOnDatabaseAsync(async context =>
            {
                TodoItem updatedTodoItem = await context.TodoItems.SingleAsync(item => item.Id == todoItemId);
                updatedTodoItem.AssigneeId.Should().Be(assigneeId);
            });
        }

        protected override HttpClient CreateClient()
        {
            return _fixture.CreateClient();
        }
    }
}
