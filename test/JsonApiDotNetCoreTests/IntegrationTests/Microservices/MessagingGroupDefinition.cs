using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices
{
    public abstract class MessagingGroupDefinition : HitCountingResourceDefinition<DomainGroup, Guid>
    {
        private readonly DbSet<DomainUser> _userSet;
        private readonly DbSet<DomainGroup> _groupSet;
        private readonly List<OutgoingMessage> _pendingMessages = new();

        private string? _beforeGroupName;

        protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Writing;

        protected MessagingGroupDefinition(IResourceGraph resourceGraph, DbSet<DomainUser> userSet, DbSet<DomainGroup> groupSet,
            ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, hitCounter)
        {
            _userSet = userSet;
            _groupSet = groupSet;
        }

        public override async Task OnPrepareWriteAsync(DomainGroup group, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnPrepareWriteAsync(group, writeOperation, cancellationToken);

            if (writeOperation == WriteOperationKind.CreateResource)
            {
                group.Id = Guid.NewGuid();
            }
            else if (writeOperation == WriteOperationKind.UpdateResource)
            {
                _beforeGroupName = group.Name;
            }
        }

        public override async Task OnSetToManyRelationshipAsync(DomainGroup group, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnSetToManyRelationshipAsync(group, hasManyRelationship, rightResourceIds, writeOperation, cancellationToken);

            if (hasManyRelationship.Property.Name == nameof(DomainGroup.Users))
            {
                HashSet<Guid> rightUserIds = rightResourceIds.Select(resource => (Guid)resource.GetTypedId()).ToHashSet();

                List<DomainUser> beforeUsers = await _userSet.Include(user => user.Group).Where(user => rightUserIds.Contains(user.Id))
                    .ToListAsync(cancellationToken);

                foreach (DomainUser beforeUser in beforeUsers)
                {
                    IMessageContent? content = null;

                    if (beforeUser.Group == null)
                    {
                        content = new UserAddedToGroupContent(beforeUser.Id, group.Id);
                    }
                    else if (beforeUser.Group != null && beforeUser.Group.Id != group.Id)
                    {
                        content = new UserMovedToGroupContent(beforeUser.Id, beforeUser.Group.Id, group.Id);
                    }

                    if (content != null)
                    {
                        var message = OutgoingMessage.CreateFromContent(content);
                        _pendingMessages.Add(message);
                    }
                }

                foreach (DomainUser userToRemoveFromGroup in group.Users.Where(user => !rightUserIds.Contains(user.Id)))
                {
                    var content = new UserRemovedFromGroupContent(userToRemoveFromGroup.Id, group.Id);
                    var message = OutgoingMessage.CreateFromContent(content);
                    _pendingMessages.Add(message);
                }
            }
        }

        public override async Task OnAddToRelationshipAsync(Guid groupId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            await base.OnAddToRelationshipAsync(groupId, hasManyRelationship, rightResourceIds, cancellationToken);

            if (hasManyRelationship.Property.Name == nameof(DomainGroup.Users))
            {
                HashSet<Guid> rightUserIds = rightResourceIds.Select(resource => (Guid)resource.GetTypedId()).ToHashSet();

                List<DomainUser> beforeUsers = await _userSet.Include(user => user.Group).Where(user => rightUserIds.Contains(user.Id))
                    .ToListAsync(cancellationToken);

                foreach (DomainUser beforeUser in beforeUsers)
                {
                    IMessageContent? content = null;

                    if (beforeUser.Group == null)
                    {
                        content = new UserAddedToGroupContent(beforeUser.Id, groupId);
                    }
                    else if (beforeUser.Group != null && beforeUser.Group.Id != groupId)
                    {
                        content = new UserMovedToGroupContent(beforeUser.Id, beforeUser.Group.Id, groupId);
                    }

                    if (content != null)
                    {
                        var message = OutgoingMessage.CreateFromContent(content);
                        _pendingMessages.Add(message);
                    }
                }
            }
        }

        public override async Task OnRemoveFromRelationshipAsync(DomainGroup group, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            await base.OnRemoveFromRelationshipAsync(group, hasManyRelationship, rightResourceIds, cancellationToken);

            if (hasManyRelationship.Property.Name == nameof(DomainGroup.Users))
            {
                HashSet<Guid> rightUserIds = rightResourceIds.Select(resource => (Guid)resource.GetTypedId()).ToHashSet();

                foreach (DomainUser userToRemoveFromGroup in group.Users.Where(user => rightUserIds.Contains(user.Id)))
                {
                    var content = new UserRemovedFromGroupContent(userToRemoveFromGroup.Id, group.Id);
                    var message = OutgoingMessage.CreateFromContent(content);
                    _pendingMessages.Add(message);
                }
            }
        }

        protected async Task FinishWriteAsync(DomainGroup group, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation == WriteOperationKind.CreateResource)
            {
                var message = OutgoingMessage.CreateFromContent(new GroupCreatedContent(group.Id, group.Name));
                await FlushMessageAsync(message, cancellationToken);
            }
            else if (writeOperation == WriteOperationKind.UpdateResource)
            {
                if (_beforeGroupName != group.Name)
                {
                    var message = OutgoingMessage.CreateFromContent(new GroupRenamedContent(group.Id, _beforeGroupName!, group.Name));
                    await FlushMessageAsync(message, cancellationToken);
                }
            }
            else if (writeOperation == WriteOperationKind.DeleteResource)
            {
                DomainGroup? groupToDelete = await GetGroupToDeleteAsync(group.Id, cancellationToken);

                if (groupToDelete != null)
                {
                    foreach (DomainUser user in groupToDelete.Users)
                    {
                        var removeMessage = OutgoingMessage.CreateFromContent(new UserRemovedFromGroupContent(user.Id, group.Id));
                        await FlushMessageAsync(removeMessage, cancellationToken);
                    }
                }

                var deleteMessage = OutgoingMessage.CreateFromContent(new GroupDeletedContent(group.Id));
                await FlushMessageAsync(deleteMessage, cancellationToken);
            }

            foreach (OutgoingMessage nextMessage in _pendingMessages)
            {
                await FlushMessageAsync(nextMessage, cancellationToken);
            }
        }

        protected abstract Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken);

        protected virtual async Task<DomainGroup?> GetGroupToDeleteAsync(Guid groupId, CancellationToken cancellationToken)
        {
            return await _groupSet.Include(group => group.Users).FirstOrDefaultAsync(group => group.Id == groupId, cancellationToken);
        }
    }
}
