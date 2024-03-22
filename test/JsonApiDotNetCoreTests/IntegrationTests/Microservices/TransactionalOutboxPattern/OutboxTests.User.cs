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
    public async Task Create_user_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        string newLoginName = _fakers.DomainUser.Generate().LoginName;
        string newDisplayName = _fakers.DomainUser.Generate().DisplayName!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainUsers",
                attributes = new
                {
                    loginName = newLoginName,
                    displayName = newDisplayName
                }
            }
        };

        const string route = "/domainUsers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("loginName").With(value => value.Should().Be(newLoginName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(newDisplayName));

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        Guid newUserId = Guid.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(1);

            var content = messages[0].GetContentAs<UserCreatedContent>();
            content.UserId.Should().Be(newUserId);
            content.UserLoginName.Should().Be(newLoginName);
            content.UserDisplayName.Should().Be(newDisplayName);
        });
    }

    [Fact]
    public async Task Create_user_in_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainGroup existingGroup = _fakers.DomainGroup.Generate();

        string newLoginName = _fakers.DomainUser.Generate().LoginName;

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
                type = "domainUsers",
                attributes = new
                {
                    loginName = newLoginName
                },
                relationships = new
                {
                    group = new
                    {
                        data = new
                        {
                            type = "domainGroups",
                            id = existingGroup.StringId
                        }
                    }
                }
            }
        };

        const string route = "/domainUsers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("loginName").With(value => value.Should().Be(newLoginName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("displayName").With(value => value.Should().BeNull());

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnSetToOneRelationshipAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        Guid newUserId = Guid.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(2);

            var content1 = messages[0].GetContentAs<UserCreatedContent>();
            content1.UserId.Should().Be(newUserId);
            content1.UserLoginName.Should().Be(newLoginName);
            content1.UserDisplayName.Should().BeNull();

            var content2 = messages[1].GetContentAs<UserAddedToGroupContent>();
            content2.UserId.Should().Be(newUserId);
            content2.GroupId.Should().Be(existingGroup.Id);
        });
    }

    [Fact]
    public async Task Update_user_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUser = _fakers.DomainUser.Generate();

        string newLoginName = _fakers.DomainUser.Generate().LoginName;
        string newDisplayName = _fakers.DomainUser.Generate().DisplayName!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainUsers",
                id = existingUser.StringId,
                attributes = new
                {
                    loginName = newLoginName,
                    displayName = newDisplayName
                }
            }
        };

        string route = $"/domainUsers/{existingUser.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(2);

            var content1 = messages[0].GetContentAs<UserLoginNameChangedContent>();
            content1.UserId.Should().Be(existingUser.Id);
            content1.BeforeUserLoginName.Should().Be(existingUser.LoginName);
            content1.AfterUserLoginName.Should().Be(newLoginName);

            var content2 = messages[1].GetContentAs<UserDisplayNameChangedContent>();
            content2.UserId.Should().Be(existingUser.Id);
            content2.BeforeUserDisplayName.Should().Be(existingUser.DisplayName);
            content2.AfterUserDisplayName.Should().Be(newDisplayName);
        });
    }

    [Fact]
    public async Task Update_user_clear_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUser = _fakers.DomainUser.Generate();
        existingUser.Group = _fakers.DomainGroup.Generate();

        string newDisplayName = _fakers.DomainUser.Generate().DisplayName!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainUsers",
                id = existingUser.StringId,
                attributes = new
                {
                    displayName = newDisplayName
                },
                relationships = new
                {
                    group = new
                    {
                        data = (object?)null
                    }
                }
            }
        };

        string route = $"/domainUsers/{existingUser.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnSetToOneRelationshipAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(2);

            var content1 = messages[0].GetContentAs<UserDisplayNameChangedContent>();
            content1.UserId.Should().Be(existingUser.Id);
            content1.BeforeUserDisplayName.Should().Be(existingUser.DisplayName);
            content1.AfterUserDisplayName.Should().Be(newDisplayName);

            var content2 = messages[1].GetContentAs<UserRemovedFromGroupContent>();
            content2.UserId.Should().Be(existingUser.Id);
            content2.GroupId.Should().Be(existingUser.Group.Id);
        });
    }

    [Fact]
    public async Task Update_user_add_to_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUser = _fakers.DomainUser.Generate();
        DomainGroup existingGroup = _fakers.DomainGroup.Generate();

        string newDisplayName = _fakers.DomainUser.Generate().DisplayName!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.AddInRange(existingUser, existingGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainUsers",
                id = existingUser.StringId,
                attributes = new
                {
                    displayName = newDisplayName
                },
                relationships = new
                {
                    group = new
                    {
                        data = new
                        {
                            type = "domainGroups",
                            id = existingGroup.StringId
                        }
                    }
                }
            }
        };

        string route = $"/domainUsers/{existingUser.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnSetToOneRelationshipAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(2);

            var content1 = messages[0].GetContentAs<UserDisplayNameChangedContent>();
            content1.UserId.Should().Be(existingUser.Id);
            content1.BeforeUserDisplayName.Should().Be(existingUser.DisplayName);
            content1.AfterUserDisplayName.Should().Be(newDisplayName);

            var content2 = messages[1].GetContentAs<UserAddedToGroupContent>();
            content2.UserId.Should().Be(existingUser.Id);
            content2.GroupId.Should().Be(existingGroup.Id);
        });
    }

    [Fact]
    public async Task Update_user_move_to_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUser = _fakers.DomainUser.Generate();
        existingUser.Group = _fakers.DomainGroup.Generate();

        DomainGroup existingGroup = _fakers.DomainGroup.Generate();

        string newDisplayName = _fakers.DomainUser.Generate().DisplayName!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.AddInRange(existingUser, existingGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainUsers",
                id = existingUser.StringId,
                attributes = new
                {
                    displayName = newDisplayName
                },
                relationships = new
                {
                    group = new
                    {
                        data = new
                        {
                            type = "domainGroups",
                            id = existingGroup.StringId
                        }
                    }
                }
            }
        };

        string route = $"/domainUsers/{existingUser.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnSetToOneRelationshipAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(2);

            var content1 = messages[0].GetContentAs<UserDisplayNameChangedContent>();
            content1.UserId.Should().Be(existingUser.Id);
            content1.BeforeUserDisplayName.Should().Be(existingUser.DisplayName);
            content1.AfterUserDisplayName.Should().Be(newDisplayName);

            var content2 = messages[1].GetContentAs<UserMovedToGroupContent>();
            content2.UserId.Should().Be(existingUser.Id);
            content2.BeforeGroupId.Should().Be(existingUser.Group.Id);
            content2.AfterGroupId.Should().Be(existingGroup.Id);
        });
    }

    [Fact]
    public async Task Delete_user_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUser = _fakers.DomainUser.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/domainUsers/{existingUser.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(1);

            var content = messages[0].GetContentAs<UserDeletedContent>();
            content.UserId.Should().Be(existingUser.Id);
        });
    }

    [Fact]
    public async Task Delete_user_in_group_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUser = _fakers.DomainUser.Generate();
        existingUser.Group = _fakers.DomainGroup.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/domainUsers/{existingUser.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(2);

            var content1 = messages[0].GetContentAs<UserRemovedFromGroupContent>();
            content1.UserId.Should().Be(existingUser.Id);
            content1.GroupId.Should().Be(existingUser.Group.Id);

            var content2 = messages[1].GetContentAs<UserDeletedContent>();
            content2.UserId.Should().Be(existingUser.Id);
        });
    }

    [Fact]
    public async Task Clear_group_from_user_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUser = _fakers.DomainUser.Generate();
        existingUser.Group = _fakers.DomainGroup.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/domainUsers/{existingUser.StringId}/relationships/group";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnSetToOneRelationshipAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(1);

            var content = messages[0].GetContentAs<UserRemovedFromGroupContent>();
            content.UserId.Should().Be(existingUser.Id);
            content.GroupId.Should().Be(existingUser.Group.Id);
        });
    }

    [Fact]
    public async Task Assign_group_to_user_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUser = _fakers.DomainUser.Generate();
        DomainGroup existingGroup = _fakers.DomainGroup.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.AddInRange(existingUser, existingGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainGroups",
                id = existingGroup.StringId
            }
        };

        string route = $"/domainUsers/{existingUser.StringId}/relationships/group";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnSetToOneRelationshipAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(1);

            var content = messages[0].GetContentAs<UserAddedToGroupContent>();
            content.UserId.Should().Be(existingUser.Id);
            content.GroupId.Should().Be(existingGroup.Id);
        });
    }

    [Fact]
    public async Task Replace_group_for_user_writes_to_outbox()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        DomainUser existingUser = _fakers.DomainUser.Generate();
        existingUser.Group = _fakers.DomainGroup.Generate();

        DomainGroup existingGroup = _fakers.DomainGroup.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<OutgoingMessage>();
            dbContext.AddInRange(existingUser, existingGroup);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "domainGroups",
                id = existingGroup.StringId
            }
        };

        string route = $"/domainUsers/{existingUser.StringId}/relationships/group";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnPrepareWriteAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnSetToOneRelationshipAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWritingAsync),
            (typeof(DomainUser), ResourceDefinitionExtensibilityPoints.OnWriteSucceededAsync)
        }, options => options.WithStrictOrdering());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<OutgoingMessage> messages = await dbContext.OutboxMessages.OrderBy(message => message.Id).ToListAsync();
            messages.ShouldHaveCount(1);

            var content = messages[0].GetContentAs<UserMovedToGroupContent>();
            content.UserId.Should().Be(existingUser.Id);
            content.BeforeGroupId.Should().Be(existingUser.Group.Id);
            content.AfterGroupId.Should().Be(existingGroup.Id);
        });
    }
}
