using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.TransactionalOutboxPattern;

public sealed partial class OutboxTests
{
    [Fact]
    public async Task Create_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        string newGroupName = _fakers.DomainGroup.GenerateOne().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainGroups",
                attributes = new
                {
                    name = newGroupName
                }
            }
        };

        const string route = "/domainGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(newGroupName);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        Guid newGroupId = Guid.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.Should().HaveCount(1);

            var content = messages[0].GetContentAs<GroupCreatedContent>();
            content.GroupId.Should().Be(newGroupId);
            content.GroupName.Should().Be(newGroupName);
        });
    }

    [Fact]
    public async Task Create_group_with_users_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUserWithoutGroup = _fakers.DomainUser.GenerateOne();

        DomainUser existingUserWithOtherGroup = _fakers.DomainUser.GenerateOne();
        existingUserWithOtherGroup.Group = _fakers.DomainGroup.GenerateOne();

        string newGroupName = _fakers.DomainGroup.GenerateOne().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.AddRange(existingUserWithoutGroup, existingUserWithOtherGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainGroups",
                attributes = new
                {
                    name = newGroupName
                },
                relationships = new
                {
                    users = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "domainUsers",
                                id = existingUserWithoutGroup.StringId
                            },
                            new
                            {
                                type = "domainUsers",
                                id = existingUserWithOtherGroup.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/domainGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(newGroupName);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnSetToManyRelationshipAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        Guid newGroupId = Guid.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.Should().HaveCount(3);

            var content1 = messages[0].GetContentAs<GroupCreatedContent>();
            content1.GroupId.Should().Be(newGroupId);
            content1.GroupName.Should().Be(newGroupName);

            var content2 = messages[1].GetContentAs<UserAddedToGroupContent>();
            content2.UserId.Should().Be(existingUserWithoutGroup.Id);
            content2.GroupId.Should().Be(newGroupId);

            var content3 = messages[2].GetContentAs<UserMovedToGroupContent>();
            content3.UserId.Should().Be(existingUserWithOtherGroup.Id);
            content3.BeforeGroupId.Should().Be(existingUserWithOtherGroup.Group.Id);
            content3.AfterGroupId.Should().Be(newGroupId);
        });
    }

    [Fact]
    public async Task Update_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainGroup existingGroup = _fakers.DomainGroup.GenerateOne();

        string newGroupName = _fakers.DomainGroup.GenerateOne().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Groups.Add(existingGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainGroups",
                id = existingGroup.StringId,
                attributes = new
                {
                    name = newGroupName
                }
            }
        };

        string route = $"/domainGroups/{existingGroup.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.Should().HaveCount(1);

            var content = messages[0].GetContentAs<GroupRenamedContent>();
            content.GroupId.Should().Be(existingGroup.StringId);
            content.BeforeGroupName.Should().Be(existingGroup.Name);
            content.AfterGroupName.Should().Be(newGroupName);
        });
    }

    [Fact]
    public async Task Update_group_with_users_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainGroup existingGroup = _fakers.DomainGroup.GenerateOne();

        DomainUser existingUserWithoutGroup = _fakers.DomainUser.GenerateOne();

        DomainUser existingUserWithSameGroup1 = _fakers.DomainUser.GenerateOne();
        existingUserWithSameGroup1.Group = existingGroup;

        DomainUser existingUserWithSameGroup2 = _fakers.DomainUser.GenerateOne();
        existingUserWithSameGroup2.Group = existingGroup;

        DomainUser existingUserWithOtherGroup = _fakers.DomainUser.GenerateOne();
        existingUserWithOtherGroup.Group = _fakers.DomainGroup.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.AddRange(existingUserWithoutGroup, existingUserWithSameGroup1, existingUserWithSameGroup2, existingUserWithOtherGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainGroups",
                id = existingGroup.StringId,
                relationships = new
                {
                    users = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "domainUsers",
                                id = existingUserWithoutGroup.StringId
                            },
                            new
                            {
                                type = "domainUsers",
                                id = existingUserWithSameGroup1.StringId
                            },
                            new
                            {
                                type = "domainUsers",
                                id = existingUserWithOtherGroup.StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/domainGroups/{existingGroup.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnSetToManyRelationshipAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.Should().HaveCount(3);

            var content1 = messages[0].GetContentAs<UserAddedToGroupContent>();
            content1.UserId.Should().Be(existingUserWithoutGroup.Id);
            content1.GroupId.Should().Be(existingGroup.Id);

            var content2 = messages[1].GetContentAs<UserMovedToGroupContent>();
            content2.UserId.Should().Be(existingUserWithOtherGroup.Id);
            content2.BeforeGroupId.Should().Be(existingUserWithOtherGroup.Group.Id);
            content2.AfterGroupId.Should().Be(existingGroup.Id);

            var content3 = messages[2].GetContentAs<UserRemovedFromGroupContent>();
            content3.UserId.Should().Be(existingUserWithSameGroup2.Id);
            content3.GroupId.Should().Be(existingGroup.Id);
        });
    }

    [Fact]
    public async Task Delete_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainGroup existingGroup = _fakers.DomainGroup.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Groups.Add(existingGroup);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/domainGroups/{existingGroup.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.Should().HaveCount(1);

            var content = messages[0].GetContentAs<GroupDeletedContent>();
            content.GroupId.Should().Be(existingGroup.StringId);
        });
    }

    [Fact]
    public async Task Delete_group_with_users_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainGroup existingGroup = _fakers.DomainGroup.GenerateOne();
        existingGroup.Users = _fakers.DomainUser.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Groups.Add(existingGroup);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/domainGroups/{existingGroup.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.Should().HaveCount(2);

            var content1 = messages[0].GetContentAs<UserRemovedFromGroupContent>();
            content1.UserId.Should().Be(existingGroup.Users.ElementAt(0).Id);
            content1.GroupId.Should().Be(existingGroup.StringId);

            var content2 = messages[1].GetContentAs<GroupDeletedContent>();
            content2.GroupId.Should().Be(existingGroup.StringId);
        });
    }

    [Fact]
    public async Task Replace_users_in_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainGroup existingGroup = _fakers.DomainGroup.GenerateOne();

        DomainUser existingUserWithoutGroup = _fakers.DomainUser.GenerateOne();

        DomainUser existingUserWithSameGroup1 = _fakers.DomainUser.GenerateOne();
        existingUserWithSameGroup1.Group = existingGroup;

        DomainUser existingUserWithSameGroup2 = _fakers.DomainUser.GenerateOne();
        existingUserWithSameGroup2.Group = existingGroup;

        DomainUser existingUserWithOtherGroup = _fakers.DomainUser.GenerateOne();
        existingUserWithOtherGroup.Group = _fakers.DomainGroup.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.AddRange(existingUserWithoutGroup, existingUserWithSameGroup1, existingUserWithSameGroup2, existingUserWithOtherGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "domainUsers",
                    id = existingUserWithoutGroup.StringId
                },
                new
                {
                    type = "domainUsers",
                    id = existingUserWithSameGroup1.StringId
                },
                new
                {
                    type = "domainUsers",
                    id = existingUserWithOtherGroup.StringId
                }
            }
        };

        string route = $"/domainGroups/{existingGroup.StringId}/relationships/users";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnSetToManyRelationshipAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.Should().HaveCount(3);

            var content1 = messages[0].GetContentAs<UserAddedToGroupContent>();
            content1.UserId.Should().Be(existingUserWithoutGroup.Id);
            content1.GroupId.Should().Be(existingGroup.Id);

            var content2 = messages[1].GetContentAs<UserMovedToGroupContent>();
            content2.UserId.Should().Be(existingUserWithOtherGroup.Id);
            content2.BeforeGroupId.Should().Be(existingUserWithOtherGroup.Group.Id);
            content2.AfterGroupId.Should().Be(existingGroup.Id);

            var content3 = messages[2].GetContentAs<UserRemovedFromGroupContent>();
            content3.UserId.Should().Be(existingUserWithSameGroup2.Id);
            content3.GroupId.Should().Be(existingGroup.Id);
        });
    }

    [Fact]
    public async Task Add_users_to_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainGroup existingGroup = _fakers.DomainGroup.GenerateOne();

        DomainUser existingUserWithoutGroup = _fakers.DomainUser.GenerateOne();

        DomainUser existingUserWithSameGroup = _fakers.DomainUser.GenerateOne();
        existingUserWithSameGroup.Group = existingGroup;

        DomainUser existingUserWithOtherGroup = _fakers.DomainUser.GenerateOne();
        existingUserWithOtherGroup.Group = _fakers.DomainGroup.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.AddRange(existingUserWithoutGroup, existingUserWithSameGroup, existingUserWithOtherGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "domainUsers",
                    id = existingUserWithoutGroup.StringId
                },
                new
                {
                    type = "domainUsers",
                    id = existingUserWithOtherGroup.StringId
                }
            }
        };

        string route = $"/domainGroups/{existingGroup.StringId}/relationships/users";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnAddToRelationshipAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.Should().HaveCount(2);

            var content1 = messages[0].GetContentAs<UserAddedToGroupContent>();
            content1.UserId.Should().Be(existingUserWithoutGroup.Id);
            content1.GroupId.Should().Be(existingGroup.Id);

            var content2 = messages[1].GetContentAs<UserMovedToGroupContent>();
            content2.UserId.Should().Be(existingUserWithOtherGroup.Id);
            content2.BeforeGroupId.Should().Be(existingUserWithOtherGroup.Group.Id);
            content2.AfterGroupId.Should().Be(existingGroup.Id);
        });
    }

    [Fact]
    public async Task Remove_users_from_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainGroup existingGroup = _fakers.DomainGroup.GenerateOne();

        DomainUser existingUserWithSameGroup1 = _fakers.DomainUser.GenerateOne();
        existingUserWithSameGroup1.Group = existingGroup;

        DomainUser existingUserWithSameGroup2 = _fakers.DomainUser.GenerateOne();
        existingUserWithSameGroup2.Group = existingGroup;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.AddRange(existingUserWithSameGroup1, existingUserWithSameGroup2);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "domainUsers",
                    id = existingUserWithSameGroup2.StringId
                }
            }
        };

        string route = $"/domainGroups/{existingGroup.StringId}/relationships/users";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnRemoveFromRelationshipAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainGroup), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.Should().HaveCount(1);

            var content = messages[0].GetContentAs<UserRemovedFromGroupContent>();
            content.UserId.Should().Be(existingUserWithSameGroup2.Id);
            content.GroupId.Should().Be(existingGroup.Id);
        });
    }
}
