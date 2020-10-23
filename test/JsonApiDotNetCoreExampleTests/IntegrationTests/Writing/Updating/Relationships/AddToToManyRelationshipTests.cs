using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing.Updating.Relationships
{
    public sealed class AddToToManyRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> _testContext;

        public AddToToManyRelationshipTests(IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Cannot_add_to_HasOne_relationship()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            var existingUserAccount = WriteFakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem, existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = existingUserAccount.StringId
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("Only to-many relationships can be updated through this endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'assignedTo' must be a to-many relationship.");
        }

        [Fact]
        public async Task Can_add_to_HasMany_relationship()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            existingWorkItem.Subscribers = WriteFakers.UserAccount.Generate(2).ToHashSet();

            var existingSubscriber = WriteFakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem, existingSubscriber);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = existingSubscriber.StringId
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Subscribers)
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstAsync();

                workItemInDatabase.Subscribers.Should().HaveCount(3);
                workItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingWorkItem.Subscribers.ElementAt(0).Id);
                workItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingWorkItem.Subscribers.ElementAt(1).Id);
                workItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingSubscriber.Id);
            });
        }

        [Fact]
        public async Task Can_add_to_HasManyThrough_relationship()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            existingWorkItem.WorkItemTags = new[]
            {
                new WorkItemTag
                {
                    Tag = WriteFakers.WorkTags.Generate()
                }
            };

            var existingTags = WriteFakers.WorkTags.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem);
                dbContext.AddRange(existingTags);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "workTags",
                        id = existingTags[0].StringId
                    },
                    new
                    {
                        type = "workTags",
                        id = existingTags[1].StringId
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.WorkItemTags)
                    .ThenInclude(workItemTag => workItemTag.Tag)
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstAsync();

                workItemInDatabase.WorkItemTags.Should().HaveCount(3);
                workItemInDatabase.WorkItemTags.Should().ContainSingle(workItemTag => workItemTag.Tag.Id == existingWorkItem.WorkItemTags.ElementAt(0).Tag.Id);
                workItemInDatabase.WorkItemTags.Should().ContainSingle(workItemTag => workItemTag.Tag.Id == existingTags[0].Id);
                workItemInDatabase.WorkItemTags.Should().ContainSingle(workItemTag => workItemTag.Tag.Id == existingTags[1].Id);
            });
        }

        [Fact]
        public async Task Cannot_add_for_missing_type()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        id = 99999999
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'type' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'type' element in 'data' element. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_add_for_unknown_type()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "doesNotExist",
                        id = 99999999
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body includes unknown resource type.");
            responseDocument.Errors[0].Detail.Should().StartWith("Resource of type 'doesNotExist' does not exist. - Request body: <<");
        }

        [Fact(Skip = "TODO: Fix bug that prevents this test from succeeding.")]
        public async Task Cannot_add_for_missing_ID()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts"
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'id' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'id' element in 'data' element. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_add_for_unknown_ID()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = 99999999
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A resource being assigned to a relationship does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'userAccounts' with ID '99999999' being assigned to relationship 'subscribers' does not exist.");
        }

        [Fact]
        public async Task Cannot_add_for_unknown_IDs()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = 88888888
                    },
                    new
                    {
                        type = "userAccounts",
                        id = 99999999
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(2);

            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A resource being assigned to a relationship does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'userAccounts' with ID '88888888' being assigned to relationship 'subscribers' does not exist.");

            responseDocument.Errors[1].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[1].Title.Should().Be("A resource being assigned to a relationship does not exist.");
            responseDocument.Errors[1].Detail.Should().Be("Resource of type 'userAccounts' with ID '99999999' being assigned to relationship 'subscribers' does not exist.");
        }

        [Fact]
        public async Task Cannot_add_to_unknown_resource_type_in_url()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            var existingSubscriber = WriteFakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem, existingSubscriber);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = existingSubscriber.StringId
                    }
                }
            };

            var route = $"/doesNotExist/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_add_to_unknown_resource_ID_in_url()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            var existingSubscriber = WriteFakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = existingSubscriber.StringId
                    }
                }
            };

            var route = "/workItems/99999999/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' with ID '99999999' does not exist.");
        }

        [Fact]
        public async Task Cannot_add_to_unknown_relationship_in_url()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = 99999999
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/doesNotExist";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested relationship does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' does not contain a relationship named 'doesNotExist'.");
        }

        [Fact]
        public async Task Cannot_add_for_relationship_mismatch_between_url_and_body()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            var existingSubscriber = WriteFakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem, existingSubscriber);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = existingSubscriber.StringId
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Conflict);
            responseDocument.Errors[0].Title.Should().Be("Resource type mismatch between request body and endpoint URL.");
            responseDocument.Errors[0].Detail.Should().Be($"Expected resource of type 'workTags' in POST request body at endpoint '/workItems/{existingWorkItem.StringId}/relationships/tags', instead of 'userAccounts'.");
        }

        [Fact]
        public async Task Can_add_already_attached_resource()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            existingWorkItem.Subscribers = WriteFakers.UserAccount.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = existingWorkItem.Subscribers.ElementAt(0).StringId
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Subscribers)
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstAsync();

                workItemInDatabase.Subscribers.Should().HaveCount(1);
                workItemInDatabase.Subscribers.ElementAt(0).Id.Should().Be(existingWorkItem.Subscribers.ElementAt(0).Id);
            });
        }

        [Fact]
        public async Task Can_add_with_already_attached_HasMany_resources()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            existingWorkItem.Subscribers = WriteFakers.UserAccount.Generate(2).ToHashSet();

            var existingSubscriber = WriteFakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem, existingSubscriber);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = existingWorkItem.Subscribers.ElementAt(0).StringId
                    },
                    new
                    {
                        type = "userAccounts",
                        id = existingWorkItem.Subscribers.ElementAt(1).StringId
                    },
                    new
                    {
                        type = "userAccounts",
                        id = existingSubscriber.StringId
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Subscribers)
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstAsync();

                workItemInDatabase.Subscribers.Should().HaveCount(3);
                workItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingWorkItem.Subscribers.ElementAt(0).Id);
                workItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingWorkItem.Subscribers.ElementAt(1).Id);
                workItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingSubscriber.Id);
            });
        }

        [Fact]
        public async Task Can_add_with_already_attached_HasManyThrough_resources()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            existingWorkItem.WorkItemTags = new[]
            {
                new WorkItemTag
                {
                    Tag = WriteFakers.WorkTags.Generate()
                }
            };

            var existingTag = WriteFakers.WorkTags.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem, existingTag);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "workTags",
                        id = existingWorkItem.WorkItemTags.ElementAt(0).Tag.StringId
                    },
                    new
                    {
                        type = "workTags",
                        id = existingTag.StringId
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.WorkItemTags)
                    .ThenInclude(workItemTag => workItemTag.Tag)
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstAsync();

                workItemInDatabase.WorkItemTags.Should().HaveCount(2);
                workItemInDatabase.WorkItemTags.Should().ContainSingle(workItemTag => workItemTag.Tag.Id == existingWorkItem.WorkItemTags.ElementAt(0).Tag.Id);
                workItemInDatabase.WorkItemTags.Should().ContainSingle(workItemTag => workItemTag.Tag.Id == existingTag.Id);
            });
        }

        [Fact]
        public async Task Can_add_with_duplicates()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            var existingSubscriber = WriteFakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem, existingSubscriber);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "userAccounts",
                        id = existingSubscriber.StringId
                    },
                    new
                    {
                        type = "userAccounts",
                        id = existingSubscriber.StringId
                    }
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Subscribers)
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstAsync();

                workItemInDatabase.Subscribers.Should().HaveCount(1);
                workItemInDatabase.Subscribers.ElementAt(0).Id.Should().Be(existingSubscriber.Id);
            });
        }

        [Fact]
        public async Task Can_add_with_empty_list()
        {
            // Arrange
            var existingWorkItem = WriteFakers.WorkItem.Generate();
            existingWorkItem.Subscribers = WriteFakers.UserAccount.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new object[0]
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Subscribers)
                    .Where(workItem => workItem.Id == existingWorkItem.Id)
                    .FirstAsync();

                workItemInDatabase.Subscribers.Should().HaveCount(1);
                workItemInDatabase.Subscribers.ElementAt(0).Id.Should().Be(existingWorkItem.Subscribers.ElementAt(0).Id);
            });
        }
    }
}
