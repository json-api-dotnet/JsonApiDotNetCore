using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Updating.Resources
{
    public sealed class UpdateToOneRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
        private readonly ReadWriteFakers _fakers = new();

        public UpdateToOneRelationshipTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<WorkItemsController>();
            testContext.UseController<WorkItemGroupsController>();
            testContext.UseController<RgbColorsController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<ImplicitlyChangingWorkItemDefinition>();
            });
        }

        [Fact]
        public async Task Can_clear_ManyToOne_relationship()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();
            existingWorkItem.Assignee = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
                        {
                            data = (object)null
                        }
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Assignee).FirstWithIdAsync(existingWorkItem.Id);

                workItemInDatabase.Assignee.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_create_OneToOne_relationship_from_principal_side()
        {
            // Arrange
            WorkItemGroup existingGroup = _fakers.WorkItemGroup.Generate();
            existingGroup.Color = _fakers.RgbColor.Generate();

            RgbColor existingColor = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingGroup, existingColor);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    id = existingGroup.StringId,
                    relationships = new
                    {
                        color = new
                        {
                            data = new
                            {
                                type = "rgbColors",
                                id = existingColor.StringId
                            }
                        }
                    }
                }
            };

            string route = $"/workItemGroups/{existingGroup.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                List<RgbColor> colorsInDatabase = await dbContext.RgbColors.Include(rgbColor => rgbColor.Group).ToListAsync();

                RgbColor colorInDatabase1 = colorsInDatabase.Single(color => color.Id == existingGroup.Color.Id);
                colorInDatabase1.Group.Should().BeNull();

                RgbColor colorInDatabase2 = colorsInDatabase.Single(color => color.Id == existingColor.Id);
                colorInDatabase2.Group.Should().NotBeNull();
                colorInDatabase2.Group.Id.Should().Be(existingGroup.Id);
            });
        }

        [Fact]
        public async Task Can_replace_OneToOne_relationship_from_dependent_side()
        {
            // Arrange
            List<WorkItemGroup> existingGroups = _fakers.WorkItemGroup.Generate(2);
            existingGroups[0].Color = _fakers.RgbColor.Generate();
            existingGroups[1].Color = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Groups.AddRange(existingGroups);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = existingGroups[0].Color.StringId,
                    relationships = new
                    {
                        group = new
                        {
                            data = new
                            {
                                type = "workItemGroups",
                                id = existingGroups[1].StringId
                            }
                        }
                    }
                }
            };

            string route = $"/rgbColors/{existingGroups[0].Color.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                List<WorkItemGroup> groupsInDatabase = await dbContext.Groups.Include(group => group.Color).ToListAsync();

                WorkItemGroup groupInDatabase1 = groupsInDatabase.Single(group => group.Id == existingGroups[0].Id);
                groupInDatabase1.Color.Should().BeNull();

                WorkItemGroup groupInDatabase2 = groupsInDatabase.Single(group => group.Id == existingGroups[1].Id);
                groupInDatabase2.Color.Should().NotBeNull();
                groupInDatabase2.Color.Id.Should().Be(existingGroups[0].Color.Id);

                List<RgbColor> colorsInDatabase = await dbContext.RgbColors.Include(color => color.Group).ToListAsync();

                RgbColor colorInDatabase1 = colorsInDatabase.Single(color => color.Id == existingGroups[0].Color.Id);
                colorInDatabase1.Group.Should().NotBeNull();
                colorInDatabase1.Group.Id.Should().Be(existingGroups[1].Id);

                RgbColor colorInDatabase2 = colorsInDatabase.SingleOrDefault(color => color.Id == existingGroups[1].Color.Id);
                colorInDatabase2.Should().NotBeNull();
                colorInDatabase2!.Group.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_clear_OneToOne_relationship()
        {
            // Arrange
            RgbColor existingColor = _fakers.RgbColor.Generate();
            existingColor.Group = _fakers.WorkItemGroup.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.RgbColors.AddRange(existingColor);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = existingColor.StringId,
                    relationships = new
                    {
                        group = new
                        {
                            data = (object)null
                        }
                    }
                }
            };

            string route = $"/rgbColors/{existingColor.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                RgbColor colorInDatabase = await dbContext.RgbColors.Include(color => color.Group).FirstWithIdOrDefaultAsync(existingColor.Id);

                colorInDatabase.Group.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_replace_ManyToOne_relationship()
        {
            // Arrange
            List<UserAccount> existingUserAccounts = _fakers.UserAccount.Generate(2);
            existingUserAccounts[0].AssignedItems = _fakers.WorkItem.Generate(2).ToHashSet();

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
                    id = existingUserAccounts[0].AssignedItems.ElementAt(1).StringId,
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = existingUserAccounts[1].StringId
                            }
                        }
                    }
                }
            };

            string route = $"/workItems/{existingUserAccounts[0].AssignedItems.ElementAt(1).StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                int workItemId = existingUserAccounts[0].AssignedItems.ElementAt(1).Id;

                WorkItem workItemInDatabase2 = await dbContext.WorkItems.Include(workItem => workItem.Assignee).FirstWithIdAsync(workItemId);

                workItemInDatabase2.Assignee.Should().NotBeNull();
                workItemInDatabase2.Assignee.Id.Should().Be(existingUserAccounts[1].Id);
            });
        }

        [Fact]
        public async Task Can_create_relationship_with_include()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();
            UserAccount existingUserAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingWorkItem, existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
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

            string route = $"/workItems/{existingWorkItem.StringId}?include=assignee";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            string description = $"{existingWorkItem.Description}{ImplicitlyChangingWorkItemDefinition.Suffix}";

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("workItems");
            responseDocument.Data.SingleValue.Id.Should().Be(existingWorkItem.StringId);
            responseDocument.Data.SingleValue.Attributes["description"].Should().Be(description);
            responseDocument.Data.SingleValue.Relationships.Should().NotBeEmpty();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("userAccounts");
            responseDocument.Included[0].Id.Should().Be(existingUserAccount.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(existingUserAccount.FirstName);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(existingUserAccount.LastName);
            responseDocument.Included[0].Relationships.Should().NotBeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Assignee).FirstWithIdAsync(existingWorkItem.Id);

                workItemInDatabase.Assignee.Should().NotBeNull();
                workItemInDatabase.Assignee.Id.Should().Be(existingUserAccount.Id);
            });
        }

        [Fact]
        public async Task Can_replace_relationship_with_include_and_fieldsets()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();
            existingWorkItem.Assignee = _fakers.UserAccount.Generate();

            UserAccount existingUserAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingWorkItem, existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
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

            string route = $"/workItems/{existingWorkItem.StringId}?fields[workItems]=description,assignee&include=assignee&fields[userAccounts]=lastName";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            string description = $"{existingWorkItem.Description}{ImplicitlyChangingWorkItemDefinition.Suffix}";

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("workItems");
            responseDocument.Data.SingleValue.Id.Should().Be(existingWorkItem.StringId);
            responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
            responseDocument.Data.SingleValue.Attributes["description"].Should().Be(description);
            responseDocument.Data.SingleValue.Relationships.Should().HaveCount(1);
            responseDocument.Data.SingleValue.Relationships["assignee"].Data.SingleValue.Id.Should().Be(existingUserAccount.StringId);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("userAccounts");
            responseDocument.Included[0].Id.Should().Be(existingUserAccount.StringId);
            responseDocument.Included[0].Attributes.Should().HaveCount(1);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(existingUserAccount.LastName);
            responseDocument.Included[0].Relationships.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Assignee).FirstWithIdAsync(existingWorkItem.Id);

                workItemInDatabase.Assignee.Should().NotBeNull();
                workItemInDatabase.Assignee.Id.Should().Be(existingUserAccount.Id);
            });
        }

        [Fact]
        public async Task Cannot_create_with_missing_data_in_relationship()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();
            UserAccount existingUserAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingWorkItem, existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
                        {
                        }
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: The 'data' element is required.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/data/relationships/assignee");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_with_array_data_in_relationship()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();
            UserAccount existingUserAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingWorkItem, existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new[]
                            {
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

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Expected an object or 'null' in 'data' element, instead of an array.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/data/relationships/assignee/data");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_missing_relationship_type()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                id = Unknown.StringId.For<UserAccount, long>()
                            }
                        }
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/data/relationships/assignee/data");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_unknown_relationship_type()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = Unknown.ResourceType,
                                id = Unknown.StringId.For<UserAccount, long>()
                            }
                        }
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
            error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
            error.Source.Pointer.Should().Be("/data/relationships/assignee/data/type");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_missing_relationship_ID()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "userAccounts"
                            }
                        }
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: The 'id' element is required.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/data/relationships/assignee/data");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_with_unknown_relationship_ID()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            string userAccountId = Unknown.StringId.For<UserAccount, long>();

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = userAccountId
                            }
                        }
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("A related resource does not exist.");
            error.Detail.Should().Be($"Related resource of type 'userAccounts' with ID '{userAccountId}' in relationship 'assignee' does not exist.");
            error.Source.Should().BeNull();
            error.Meta.Should().NotContainKey("requestBody");
        }

        [Fact]
        public async Task Cannot_create_on_relationship_type_mismatch()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "rgbColors",
                                id = "0A0B0C"
                            }
                        }
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Conflict);
            error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
            error.Detail.Should().Be("Type 'rgbColors' is incompatible with type 'userAccounts' of relationship 'assignee'.");
            error.Source.Pointer.Should().Be("/data/relationships/assignee/data/type");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Can_clear_cyclic_relationship()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();

                existingWorkItem.Parent = existingWorkItem;
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        parent = new
                        {
                            data = (object)null
                        }
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Parent).FirstWithIdAsync(existingWorkItem.Id);

                workItemInDatabase.Parent.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_assign_cyclic_relationship()
        {
            // Arrange
            WorkItem existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    id = existingWorkItem.StringId,
                    relationships = new
                    {
                        parent = new
                        {
                            data = new
                            {
                                type = "workItems",
                                id = existingWorkItem.StringId
                            }
                        }
                    }
                }
            };

            string route = $"/workItems/{existingWorkItem.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Parent).FirstWithIdAsync(existingWorkItem.Id);

                workItemInDatabase.Parent.Should().NotBeNull();
                workItemInDatabase.Parent.Id.Should().Be(existingWorkItem.Id);
            });
        }
    }
}
