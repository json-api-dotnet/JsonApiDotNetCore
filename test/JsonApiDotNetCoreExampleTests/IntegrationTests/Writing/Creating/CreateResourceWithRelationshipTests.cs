using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing.Creating
{
    public sealed class CreateResourceWithRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> _testContext;

        public CreateResourceWithRelationshipTests(IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Can_create_resource_with_OneToOne_relationship_from_principal_side()
        {
            // Arrange
            var existingGroup = WriteFakers.WorkItemGroup.Generate();
            existingGroup.Color = WriteFakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Groups.Add(existingGroup);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    relationships = new
                    {
                        color = new
                        {
                            data = new
                            {
                                type = "rgbColors",
                                id = existingGroup.Color.StringId
                            }
                        }
                    }
                }
            };

            var route = "/workItemGroups";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newGroupId = responseDocument.SingleData.Id;
            newGroupId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var groupsInDatabase = await dbContext.Groups
                    .Include(group => group.Color)
                    .ToListAsync();

                var newGroupInDatabase = groupsInDatabase.Single(p => p.StringId == newGroupId);
                newGroupInDatabase.Color.Should().NotBeNull();
                newGroupInDatabase.Color.Id.Should().Be(existingGroup.Color.Id);

                var existingGroupInDatabase = groupsInDatabase.Single(p => p.Id == existingGroup.Id);
                existingGroupInDatabase.Color.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_create_resource_with_OneToOne_relationship_from_dependent_side_with_implicit_remove()
        {
            // Arrange
            var existingColor = WriteFakers.RgbColor.Generate();
            existingColor.Group = WriteFakers.WorkItemGroup.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.RgbColors.Add(existingColor);
                await dbContext.SaveChangesAsync();
            });

            string colorId = "#112233";

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = colorId,
                    relationships = new
                    {
                        group = new
                        {
                            data = new
                            {
                                type = "workItemGroups",
                                id = existingColor.Group.StringId
                            }
                        }
                    }
                }
            };

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var colorsInDatabase = await dbContext.RgbColors
                    .Include(rgbColor => rgbColor.Group)
                    .ToListAsync();

                var newColorInDatabase = colorsInDatabase.Single(p => p.Id == colorId);
                newColorInDatabase.Group.Should().NotBeNull();
                newColorInDatabase.Group.Id.Should().Be(existingColor.Group.Id);

                var existingColorInDatabase = colorsInDatabase.Single(p => p.Id == existingColor.Id);
                existingColorInDatabase.Group.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_create_resource_with_HasOne_relationship_with_include()
        {
            // Arrange
            var existingUserAccount = WriteFakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.Add(existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignedTo = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = existingUserAccount.StringId
                            }
                        }
                    }
                }
            };

            var route = "/workItems?include=assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("userAccounts");
            responseDocument.Included[0].Id.Should().Be(existingUserAccount.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(existingUserAccount.FirstName);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(existingUserAccount.LastName);

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.AssignedTo)
                    .ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.AssignedTo.Should().NotBeNull();
                newWorkItemInDatabase.AssignedTo.Id.Should().Be(existingUserAccount.Id);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_HasOne_relationship_with_include_and_primary_fieldset()
        {
            // Arrange
            var existingUserAccount = WriteFakers.UserAccount.Generate();
            var workItem = WriteFakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.Add(existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        description = workItem.Description,
                        priority = workItem.Priority
                    },
                    relationships = new
                    {
                        assignedTo = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = existingUserAccount.StringId
                            }
                        }
                    }
                }
            };

            var route = "/workItems?fields=description&include=assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["description"].Should().Be(workItem.Description);

            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("userAccounts");
            responseDocument.Included[0].Id.Should().Be(existingUserAccount.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(existingUserAccount.FirstName);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(existingUserAccount.LastName);

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.AssignedTo)
                    .ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.Description.Should().Be(workItem.Description);
                newWorkItemInDatabase.Priority.Should().Be(workItem.Priority);
                newWorkItemInDatabase.AssignedTo.Should().NotBeNull();
                newWorkItemInDatabase.AssignedTo.Id.Should().Be(existingUserAccount.Id);
            });
        }

        [Fact]
        public async Task Cannot_create_resource_for_missing_HasOne_relationship_type()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignedTo = new
                        {
                            data = new
                            {
                                id = "12345678"
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'type' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'type' element in relationship 'assignedTo'. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_resource_for_missing_HasOne_relationship_ID()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignedTo = new
                        {
                            data = new
                            {
                                type = "userAccounts"
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'id' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'id' element in relationship 'assignedTo'. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_resource_for_unknown_HasOne_relationship_ID()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignedTo = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = "12345678"
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A resource being assigned to a relationship does not exist.");
            responseDocument.Errors[0].Detail.Should().StartWith("Resource of type 'userAccounts' with ID '12345678' being assigned to relationship 'assignedTo' does not exist.");
        }

        [Fact]
        public async Task Can_create_resource_with_HasMany_relationship()
        {
            // Arrange
            var existingUserAccounts = WriteFakers.UserAccount.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.AddRange(existingUserAccounts);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[0].StringId
                                },
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[1].StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Included.Should().BeNull();

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Subscribers)
                    .ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.Subscribers.Should().HaveCount(2);
                newWorkItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingUserAccounts[0].Id);
                newWorkItemInDatabase.Subscribers.Should().ContainSingle(subscriber => subscriber.Id == existingUserAccounts[1].Id);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_HasMany_relationship_with_include()
        {
            // Arrange
            var existingUserAccounts = WriteFakers.UserAccount.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.AddRange(existingUserAccounts);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[0].StringId
                                },
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[1].StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems?include=subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();
            
            responseDocument.Included.Should().HaveCount(2);
            responseDocument.Included.Should().OnlyContain(p => p.Type == "userAccounts");
            responseDocument.Included.Should().ContainSingle(p => p.Id == existingUserAccounts[0].StringId);
            responseDocument.Included.Should().ContainSingle(p => p.Id == existingUserAccounts[1].StringId);
            responseDocument.Included.Should().OnlyContain(p => p.Attributes["firstName"] != null);
            responseDocument.Included.Should().OnlyContain(p => p.Attributes["lastName"] != null);

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Subscribers)
                    .ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.Subscribers.Should().HaveCount(2);
                newWorkItemInDatabase.Subscribers.Should().ContainSingle(p => p.Id == existingUserAccounts[0].Id);
                newWorkItemInDatabase.Subscribers.Should().ContainSingle(p => p.Id == existingUserAccounts[1].Id);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_HasMany_relationship_with_include_and_secondary_fieldset()
        {
            // Arrange
            var existingUserAccounts = WriteFakers.UserAccount.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.AddRange(existingUserAccounts);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[0].StringId
                                },
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[1].StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems?include=subscribers&fields[subscribers]=firstName";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();
            
            responseDocument.Included.Should().HaveCount(2);
            responseDocument.Included.Should().OnlyContain(p => p.Type == "userAccounts");
            responseDocument.Included.Should().ContainSingle(p => p.Id == existingUserAccounts[0].StringId);
            responseDocument.Included.Should().ContainSingle(p => p.Id == existingUserAccounts[1].StringId);
            responseDocument.Included.Should().OnlyContain(p => p.Attributes.Count == 1);
            responseDocument.Included.Should().OnlyContain(p => p.Attributes["firstName"] != null);

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Subscribers)
                    .ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.Subscribers.Should().HaveCount(2);
                newWorkItemInDatabase.Subscribers.Should().ContainSingle(p => p.Id == existingUserAccounts[0].Id);
                newWorkItemInDatabase.Subscribers.Should().ContainSingle(p => p.Id == existingUserAccounts[1].Id);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_duplicate_HasMany_relationships()
        {
            // Arrange
            var existingUserAccount = WriteFakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.Add(existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccount.StringId
                                },
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccount.StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems?include=subscribers";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();
            
            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("userAccounts");
            responseDocument.Included[0].Id.Should().Be(existingUserAccount.StringId);

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Subscribers)
                    .ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.Subscribers.Should().HaveCount(1);
                newWorkItemInDatabase.Subscribers.Single().Id.Should().Be(existingUserAccount.Id);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_HasManyThrough_relationship_with_include_and_fieldsets()
        {
            // Arrange
            var existingTags = WriteFakers.WorkTags.Generate(3);
            var workItemToCreate = WriteFakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkTags.AddRange(existingTags);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        description = workItemToCreate.Description,
                        priority = workItemToCreate.Priority
                    },
                    relationships = new
                    {
                        tags = new
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
                                },
                                new
                                {
                                    type = "workTags",
                                    id = existingTags[2].StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems?fields=priority&include=tags&fields[tags]=text";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["priority"].Should().Be(workItemToCreate.Priority.ToString("G"));

            responseDocument.SingleData.Relationships.Should().NotBeEmpty();
            
            responseDocument.Included.Should().HaveCount(3);
            responseDocument.Included.Should().OnlyContain(p => p.Type == "workTags");
            responseDocument.Included.Should().ContainSingle(p => p.Id == existingTags[0].StringId);
            responseDocument.Included.Should().ContainSingle(p => p.Id == existingTags[1].StringId);
            responseDocument.Included.Should().ContainSingle(p => p.Id == existingTags[2].StringId);
            responseDocument.Included.Should().OnlyContain(p => p.Attributes.Count == 1);
            responseDocument.Included.Should().OnlyContain(p => p.Attributes["text"] != null);

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.WorkItemTags)
                    .ThenInclude(workItemTag => workItemTag.Tag)
                    .ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.WorkItemTags.Should().HaveCount(3);
                newWorkItemInDatabase.WorkItemTags.Should().ContainSingle(p => p.Tag.Id == existingTags[0].Id);
                newWorkItemInDatabase.WorkItemTags.Should().ContainSingle(p => p.Tag.Id == existingTags[1].Id);
                newWorkItemInDatabase.WorkItemTags.Should().ContainSingle(p => p.Tag.Id == existingTags[2].Id);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_unknown_relationship()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        doesNotExist = new
                        {
                            data = new
                            {
                                type = "doesNotExist",
                                id = "12345678"
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems.ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.Description.Should().BeNull();
            });
        }

        [Fact]
        public async Task Cannot_create_resource_for_missing_HasMany_relationship_type()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    id = "12345678"
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'type' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'type' element in relationship 'subscribers'. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_resource_for_missing_HasMany_relationship_ID()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts"
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'id' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'id' element in relationship 'subscribers'. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_resource_for_unknown_HasMany_relationship_IDs()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    relationships = new
                    {
                        assignedItems = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "workItems",
                                    id = "12345678"
                                },
                                new
                                {
                                    type = "workItems",
                                    id = "87654321"
                                }
                            }
                        }
                    }
                }
            };

            var route = "/userAccounts";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(2);

            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A resource being assigned to a relationship does not exist.");
            responseDocument.Errors[0].Detail.Should().StartWith("Resource of type 'workItems' with ID '12345678' being assigned to relationship 'assignedItems' does not exist.");

            responseDocument.Errors[1].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[1].Title.Should().Be("A resource being assigned to a relationship does not exist.");
            responseDocument.Errors[1].Detail.Should().StartWith("Resource of type 'workItems' with ID '87654321' being assigned to relationship 'assignedItems' does not exist.");
        }

        [Fact]
        public async Task Can_create_resource_with_multiple_relationship_types()
        {
            // Arrange
            var existingUserAccounts = WriteFakers.UserAccount.Generate(2);
            var existingTag = WriteFakers.WorkTags.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.AddRange(existingUserAccounts);
                dbContext.WorkTags.Add(existingTag);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignedTo = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = existingUserAccounts[0].StringId
                            }
                        },
                        subscribers = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccounts[1].StringId
                                }
                            }
                        },
                        tags = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "workTags",
                                    id = existingTag.StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.AssignedTo)
                    .Include(workItem => workItem.Subscribers)
                    .Include(workItem => workItem.WorkItemTags)
                    .ThenInclude(workItemTag => workItemTag.Tag)
                    .ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.AssignedTo.Should().NotBeNull();
                newWorkItemInDatabase.AssignedTo.Id.Should().Be(existingUserAccounts[0].Id);
                newWorkItemInDatabase.Subscribers.Should().HaveCount(1);
                newWorkItemInDatabase.Subscribers.Single().Id.Should().Be(existingUserAccounts[1].Id);
                newWorkItemInDatabase.WorkItemTags.Should().HaveCount(1);
                newWorkItemInDatabase.WorkItemTags.Single().Tag.Id.Should().Be(existingTag.Id);
            });
        }
    }
}
