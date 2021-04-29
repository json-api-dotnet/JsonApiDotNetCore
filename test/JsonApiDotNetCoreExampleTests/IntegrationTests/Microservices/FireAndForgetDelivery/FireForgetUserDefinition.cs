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
    public sealed class FireForgetUserDefinition : MessagingUserDefinition
    {
        private readonly MessageBroker _messageBroker;
        private DomainUser _userToDelete;

        public FireForgetUserDefinition(IResourceGraph resourceGraph, FireForgetDbContext dbContext, MessageBroker messageBroker)
            : base(resourceGraph, dbContext.Users)
        {
            _messageBroker = messageBroker;
        }

        public override async Task OnWritingAsync(DomainUser user, OperationKind operationKind, CancellationToken cancellationToken)
        {
            if (operationKind == OperationKind.DeleteResource)
            {
                _userToDelete = await base.GetUserToDeleteAsync(user.Id, cancellationToken);
            }
        }

        public override Task OnWriteSucceededAsync(DomainUser user, OperationKind operationKind, CancellationToken cancellationToken)
        {
            return FinishWriteAsync(user, operationKind, cancellationToken);
        }

        protected override Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            return _messageBroker.PostMessageAsync(message, cancellationToken);
        }

        protected override Task<DomainUser> GetUserToDeleteAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_userToDelete);
        }
    }
}
