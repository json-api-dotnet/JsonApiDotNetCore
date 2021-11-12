using System;
using System.Collections.Generic;
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
    public sealed class PersonTests : IntegrationTest, IClassFixture<CosmosDbFixture>
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

        public PersonTests(CosmosDbFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// GetAsync(CancellationToken cancellationToken)
        /// </summary>
        [Fact]
        public async Task Can_get_primary_resources()
        {
            // Arrange
            const string route = "/api/v1/people";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldNotBeNull();
            responseDocument.Data.ManyValue.Should().HaveCountGreaterOrEqualTo(2);
        }

        /// <summary>
        /// GetAsync(CancellationToken cancellationToken)
        /// </summary>
        [Fact]
        public async Task Can_get_primary_resources_with_filter()
        {
            // Arrange
            const string fieldName = "firstName";
            const string fieldValue = "Clyde";
            string route = $"/api/v1/people?filter=equals({fieldName},'{fieldValue}')";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldNotBeNull().ShouldHaveCount(1);
            responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey(fieldName).With(value => value.Should().Be(fieldValue));
        }

        /// <summary>
        /// GetAsync(CancellationToken cancellationToken)
        /// </summary>
        [Theory]
        [InlineData("ownedTodoItems")]
        [InlineData("assignedTodoItems")]
        public async Task Can_get_primary_resources_including_secondary_resources(string relationshipName)
        {
            // Arrange
            string route = $"/api/v1/people?include={relationshipName}";

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
        [Fact]
        public async Task Can_get_primary_resource_by_id()
        {
            // Arrange
            string route = $"/api/v1/people/{_fixture.BonnieId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("firstName").With(value => value.Should().Be("Bonnie"));
        }

        /// <summary>
        /// GetAsync(TId id, CancellationToken cancellationToken)
        /// </summary>
        [Theory]
        [InlineData("ownedTodoItems")]
        [InlineData("assignedTodoItems")]
        public async Task Can_get_primary_resource_by_id_including_secondary_resources(string relationshipName)
        {
            // Arrange
            string route = $"/api/v1/people/{_fixture.BonnieId}?include={relationshipName}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("firstName").With(value => value.Should().Be("Bonnie"));

            responseDocument.Included.Should().NotBeNull().And.NotBeEmpty();
        }

        /// <summary>
        /// GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        /// </summary>
        [Theory]
        [InlineData("ownedTodoItems")]
        [InlineData("assignedTodoItems")]
        public async Task Can_get_secondary_resources_related_to_primary_resource(string relationshipName)
        {
            // Arrange
            string route = $"/api/v1/people/{_fixture.BonnieId}/{relationshipName}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.Should().NotBeNull().And.NotBeEmpty();

            responseDocument.Included.Should().BeNull();
        }

        /// <summary>
        /// GetRelationshipAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        /// </summary>
        [Theory]
        [InlineData("ownedTodoItems")]
        [InlineData("assignedTodoItems")]
        public async Task Can_get_relationships_of_primary_resource(string relationshipName)
        {
            // Arrange
            string route = $"/api/v1/people/{_fixture.BonnieId}/relationships/{relationshipName}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldNotBeNull();

            foreach (ResourceObject resourceObject in responseDocument.Data.ManyValue)
            {
                resourceObject.Type.ShouldNotBeNull();
                resourceObject.Id.ShouldNotBeNull();
                resourceObject.Attributes.Should().BeNull();
                resourceObject.Relationships.Should().BeNull();
            }

            responseDocument.Included.Should().BeNull();
        }

        /// <summary>
        /// CreateAsync(TResource resource, CancellationToken cancellationToken)
        /// </summary>
        [Fact]
        public async Task Can_create_primary_resource()
        {
            // Arrange
            const string route = "/api/v1/people";

            var person = new Person
            {
                FirstName = "Mad",
                LastName = "Max"
            };

            var requestBody = new
            {
                data = new
                {
                    type = "people",
                    attributes = new
                    {
                        firstName = person.FirstName,
                        lastName = person.LastName
                    }
                }
            };

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("firstName").With(value => value.Should().Be(person.FirstName));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(person.LastName));
        }

        /// <summary>
        /// AddToToManyRelationshipAsync(TId primaryId, string relationshipName, ISet{IIdentifiable} secondaryResourceIds, ...)
        /// </summary>
        [Fact]
        public async Task Can_add_existing_secondary_resource_to_many_relationship()
        {
            // Arrange
            TodoItem todoItem = await _fixture.CreateEntityAsync(new TodoItem
            {
                Id = Guid.NewGuid(),
                Description = "Buy booze",
                OwnerId = _fixture.ClydeId,
                Tags = new HashSet<Tag>
                {
                    new()
                    {
                        Name = "Errand"
                    }
                }
            });

            todoItem.AssigneeId.Should().BeNull();

            Guid assigneeId = _fixture.BonnieId;
            string route = $"/api/v1/people/{assigneeId}/relationships/assignedTodoItems";

            var requestBody = new
            {
                Data = new[]
                {
                    new
                    {
                        type = "todoItems",
                        id = todoItem.StringId
                    }
                }
            };

            // Act
            (HttpResponseMessage httpResponse, Document _) = await ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _fixture.RunOnDatabaseAsync(async context =>
            {
                TodoItem updatedTodoItem = await context.TodoItems.SingleAsync(item => item.Id == todoItem.Id);
                updatedTodoItem.AssigneeId.Should().Be(assigneeId);
            });
        }

        /// <summary>
        /// AddToToManyRelationshipAsync(TId primaryId, string relationshipName, ISet{IIdentifiable} secondaryResourceIds, ...)
        /// </summary>
        [Fact]
        public async Task Cannot_add_non_existent_secondary_resource_to_many_relationship()
        {
            string route = $"/api/v1/people/{_fixture.BonnieId}/relationships/assignedTodoItems";

            string nonExistentGuid = Guid.NewGuid().ToString();

            var requestBody = new
            {
                Data = new[]
                {
                    new
                    {
                        type = "todoItems",
                        Id = nonExistentGuid
                    }
                }
            };

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);
            responseDocument.Errors![0].Detail.Should().Contain(nonExistentGuid);
        }

        /// <summary>
        /// UpdateAsync(TId id, TResource resource, CancellationToken cancellationToken)
        /// </summary>
        [Fact]
        public async Task Can_update_primary_resource()
        {
            // Arrange
            const string unchangedFirstName = "Roger";
            const string originalLastName = "Rabbit";
            const string updatedLastName = "Rascal";

            Person person = await _fixture.CreateEntityAsync(new Person
            {
                Id = Guid.NewGuid(),
                FirstName = unchangedFirstName,
                LastName = originalLastName
            });

            string route = $"/api/v1/people/{person.StringId}";

            var requestBody = new
            {
                data = new
                {
                    type = "people",
                    id = person.StringId,
                    attributes = new
                    {
                        lastName = updatedLastName
                    }
                }
            };

            // Act
            (HttpResponseMessage httpResponse, Document _) = await ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _fixture.RunOnDatabaseAsync(async context =>
            {
                Person updatedPerson = await context.People.SingleAsync(item => item.Id == person.Id);

                updatedPerson.FirstName.Should().Be(unchangedFirstName);
                updatedPerson.LastName.Should().Be(updatedLastName);
            });
        }

        /// <summary>
        /// SetRelationshipAsync(TId leftId, string relationshipName, object? rightValue, CancellationToken cancellationToken)
        /// </summary>
        [Fact]
        public Task Can_set_relationship_of_primary_resource()
        {
            // The test cannot be performed on people since there is no to-one relationship that can be set.
            return Task.CompletedTask;
        }

        /// <summary>
        /// DeleteAsync(TId id, CancellationToken cancellationToken)
        /// </summary>
        [Fact]
        public async Task Can_delete_primary_resource()
        {
            // Arrange
            Person person = await _fixture.CreateEntityAsync(new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Bugsy",
                LastName = "Malone"
            });

            string route = $"/api/v1/people/{person.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document _) = await ExecuteDeleteAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _fixture.RunOnDatabaseAsync(async context =>
            {
                Person? deletedPerson = await context.People.SingleOrDefaultAsync(item => item.Id == person.Id);
                deletedPerson.Should().BeNull();
            });
        }

        /// <summary>
        /// RemoveFromToManyRelationshipAsync(TId leftId, string relationshipName, ISet{IIdentifiable} rightResourceIds, ...)
        /// </summary>
        [Fact]
        public async Task Can_remove_existing_secondary_resource_from_to_many_relationship()
        {
            // Arrange
            Guid assigneeId = _fixture.BonnieId;

            TodoItem todoItem = await _fixture.CreateEntityAsync(new TodoItem
            {
                Id = Guid.NewGuid(),
                Description = "Buy gun",
                OwnerId = _fixture.ClydeId,
                AssigneeId = assigneeId,
                Tags = new HashSet<Tag>
                {
                    new()
                    {
                        Name = "Errand"
                    }
                }
            });

            string route = $"/api/v1/people/{assigneeId}/relationships/assignedTodoItems";

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "todoItems",
                        id = todoItem.StringId
                    }
                }
            };

            // Act
            (HttpResponseMessage httpResponse, Document _) = await ExecuteDeleteAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _fixture.RunOnDatabaseAsync(async context =>
            {
                TodoItem? updatedTodoItem = await context.TodoItems.SingleAsync(item => item.Id == todoItem.Id);
                updatedTodoItem.AssigneeId.Should().BeNull();
            });
        }

        /// <summary>
        /// RemoveFromToManyRelationshipAsync(TId leftId, string relationshipName, ISet{IIdentifiable} rightResourceIds, ...)
        /// </summary>
        [Fact]
        public async Task Removing_non_existent_secondary_resource_from_to_many_relationship_returns_no_content_status_code()
        {
            // Arrange
            string route = $"/api/v1/people/{_fixture.BonnieId}/relationships/assignedTodoItems";

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "todoItems",
                        id = Guid.NewGuid().ToString()
                    }
                }
            };

            // Act
            (HttpResponseMessage httpResponse, Document _) = await ExecuteDeleteAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
        }

        protected override HttpClient CreateClient()
        {
            return _fixture.CreateClient();
        }
    }
}
