using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.FireAndForgetDelivery
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class FireForgetGroupDefinition : MessagingGroupDefinition
    {
        private readonly MessageBroker _messageBroker;
        private DomainGroup? _groupToDelete;

        public FireForgetGroupDefinition(IResourceGraph resourceGraph, FireForgetDbContext dbContext, MessageBroker messageBroker,
            ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, dbContext.Users, dbContext.Groups, hitCounter)
        {
            _messageBroker = messageBroker;
        }

        public override async Task OnWritingAsync(DomainGroup group, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnWritingAsync(group, writeOperation, cancellationToken);

            if (writeOperation == WriteOperationKind.DeleteResource)
            {
                _groupToDelete = await base.GetGroupToDeleteAsync(group.Id, cancellationToken);
            }
        }

        public override async Task OnWriteSucceededAsync(DomainGroup group, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnWriteSucceededAsync(group, writeOperation, cancellationToken);

            await FinishWriteAsync(group, writeOperation, cancellationToken);
        }

        protected override Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            return _messageBroker.PostMessageAsync(message, cancellationToken);
        }

        protected override Task<DomainGroup?> GetGroupToDeleteAsync(Guid groupId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_groupToDelete);
        }
    }
}
