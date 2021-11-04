using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices
{
    public abstract class MessagingUserDefinition : HitCountingResourceDefinition<DomainUser, Guid>
    {
        private readonly DbSet<DomainUser> _userSet;
        private readonly List<OutgoingMessage> _pendingMessages = new();

        private string? _beforeLoginName;
        private string? _beforeDisplayName;

        protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Writing;

        protected MessagingUserDefinition(IResourceGraph resourceGraph, DbSet<DomainUser> userSet, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, hitCounter)
        {
            _userSet = userSet;
        }

        public override async Task OnPrepareWriteAsync(DomainUser user, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnPrepareWriteAsync(user, writeOperation, cancellationToken);

            if (writeOperation == WriteOperationKind.CreateResource)
            {
                user.Id = Guid.NewGuid();
            }
            else if (writeOperation == WriteOperationKind.UpdateResource)
            {
                _beforeLoginName = user.LoginName;
                _beforeDisplayName = user.DisplayName;
            }
        }

        public override async Task<IIdentifiable?> OnSetToOneRelationshipAsync(DomainUser user, HasOneAttribute hasOneRelationship,
            IIdentifiable? rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnSetToOneRelationshipAsync(user, hasOneRelationship, rightResourceId, writeOperation, cancellationToken);

            if (hasOneRelationship.Property.Name == nameof(DomainUser.Group))
            {
                var afterGroupId = (Guid?)rightResourceId?.GetTypedId();
                IMessageContent? content = null;

                if (user.Group != null && afterGroupId == null)
                {
                    content = new UserRemovedFromGroupContent(user.Id, user.Group.Id);
                }
                else if (user.Group == null && afterGroupId != null)
                {
                    content = new UserAddedToGroupContent(user.Id, afterGroupId.Value);
                }
                else if (user.Group != null && afterGroupId != null && user.Group.Id != afterGroupId)
                {
                    content = new UserMovedToGroupContent(user.Id, user.Group.Id, afterGroupId.Value);
                }

                if (content != null)
                {
                    var message = OutgoingMessage.CreateFromContent(content);
                    _pendingMessages.Add(message);
                }
            }

            return rightResourceId;
        }

        protected async Task FinishWriteAsync(DomainUser user, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation == WriteOperationKind.CreateResource)
            {
                var content = new UserCreatedContent(user.Id, user.LoginName, user.DisplayName);
                var message = OutgoingMessage.CreateFromContent(content);
                await FlushMessageAsync(message, cancellationToken);
            }
            else if (writeOperation == WriteOperationKind.UpdateResource)
            {
                if (_beforeLoginName != user.LoginName)
                {
                    var content = new UserLoginNameChangedContent(user.Id, _beforeLoginName!, user.LoginName);
                    var message = OutgoingMessage.CreateFromContent(content);
                    await FlushMessageAsync(message, cancellationToken);
                }

                if (_beforeDisplayName != user.DisplayName)
                {
                    var content = new UserDisplayNameChangedContent(user.Id, _beforeDisplayName!, user.DisplayName);
                    var message = OutgoingMessage.CreateFromContent(content);
                    await FlushMessageAsync(message, cancellationToken);
                }
            }
            else if (writeOperation == WriteOperationKind.DeleteResource)
            {
                DomainUser? userToDelete = await GetUserToDeleteAsync(user.Id, cancellationToken);

                if (userToDelete?.Group != null)
                {
                    var content = new UserRemovedFromGroupContent(user.Id, userToDelete.Group.Id);
                    var message = OutgoingMessage.CreateFromContent(content);
                    await FlushMessageAsync(message, cancellationToken);
                }

                var deleteMessage = OutgoingMessage.CreateFromContent(new UserDeletedContent(user.Id));
                await FlushMessageAsync(deleteMessage, cancellationToken);
            }

            foreach (OutgoingMessage nextMessage in _pendingMessages)
            {
                await FlushMessageAsync(nextMessage, cancellationToken);
            }
        }

        protected abstract Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken);

        protected virtual async Task<DomainUser?> GetUserToDeleteAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _userSet.Include(domainUser => domainUser.Group).FirstOrDefaultAsync(domainUser => domainUser.Id == userId, cancellationToken);
        }
    }
}
