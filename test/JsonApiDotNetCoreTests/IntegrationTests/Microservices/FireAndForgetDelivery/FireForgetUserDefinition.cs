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
        private DomainUser? _userToDelete;

        public FireForgetUserDefinition(IResourceGraph resourceGraph, FireForgetDbContext dbContext, MessageBroker messageBroker,
            ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, dbContext.Users, hitCounter)
        {
            _messageBroker = messageBroker;
        }

        public override async Task OnWritingAsync(DomainUser user, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnWritingAsync(user, writeOperation, cancellationToken);

            if (writeOperation == WriteOperationKind.DeleteResource)
            {
                _userToDelete = await base.GetUserToDeleteAsync(user.Id, cancellationToken);
            }
        }

        public override async Task OnWriteSucceededAsync(DomainUser user, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnWriteSucceededAsync(user, writeOperation, cancellationToken);

            await FinishWriteAsync(user, writeOperation, cancellationToken);
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
