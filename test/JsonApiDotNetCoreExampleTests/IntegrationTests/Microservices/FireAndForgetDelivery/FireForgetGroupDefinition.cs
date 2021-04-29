using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreExampleTests.IntegrationTests.Microservices.Messages;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Microservices.FireAndForgetDelivery
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class FireForgetGroupDefinition : MessagingGroupDefinition
    {
        private readonly MessageBroker _messageBroker;
        private DomainGroup _groupToDelete;

        public FireForgetGroupDefinition(IResourceGraph resourceGraph, FireForgetDbContext dbContext, MessageBroker messageBroker)
            : base(resourceGraph, dbContext.Users, dbContext.Groups)
        {
            _messageBroker = messageBroker;
        }

        public override async Task OnWritingAsync(DomainGroup group, OperationKind operationKind, CancellationToken cancellationToken)
        {
            if (operationKind == OperationKind.DeleteResource)
            {
                _groupToDelete = await base.GetGroupToDeleteAsync(group.Id, cancellationToken);
            }
        }

        public override Task OnWriteSucceededAsync(DomainGroup group, OperationKind operationKind, CancellationToken cancellationToken)
        {
            return FinishWriteAsync(group, operationKind, cancellationToken);
        }

        protected override Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            return _messageBroker.PostMessageAsync(message, cancellationToken);
        }

        protected override Task<DomainGroup> GetGroupToDeleteAsync(Guid groupId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_groupToDelete);
        }
    }
}
