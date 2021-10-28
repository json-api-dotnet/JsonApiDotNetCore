using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.FireAndForgetDelivery
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class FireForgetUserDefinition : MessagingUserDefinition
    {
        private readonly MessageBroker _messageBroker;
        private readonly ResourceDefinitionHitCounter _hitCounter;
        private DomainUser? _userToDelete;

        public FireForgetUserDefinition(IResourceGraph resourceGraph, FireForgetDbContext dbContext, MessageBroker messageBroker,
            ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, dbContext.Users, hitCounter)
        {
            _messageBroker = messageBroker;
            _hitCounter = hitCounter;
        }

        public override async Task OnWritingAsync(DomainUser user, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            _hitCounter.TrackInvocation<DomainUser>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnWritingAsync);

            if (writeOperation == WriteOperationKind.DeleteResource)
            {
                _userToDelete = await base.GetUserToDeleteAsync(user.Id, cancellationToken);
            }
        }

        public override Task OnWriteSucceededAsync(DomainUser user, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            _hitCounter.TrackInvocation<DomainUser>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnWriteSucceededAsync);

            return FinishWriteAsync(user, writeOperation, cancellationToken);
        }

        protected override Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            return _messageBroker.PostMessageAsync(message, cancellationToken);
        }

        protected override Task<DomainUser?> GetUserToDeleteAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_userToDelete);
        }
    }
}
