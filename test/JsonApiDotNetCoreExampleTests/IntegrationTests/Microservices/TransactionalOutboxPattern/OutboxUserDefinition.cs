using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreExampleTests.IntegrationTests.Microservices.Messages;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Microservices.TransactionalOutboxPattern
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class OutboxUserDefinition : MessagingUserDefinition
    {
        private readonly DbSet<OutgoingMessage> _outboxMessageSet;

        public OutboxUserDefinition(IResourceGraph resourceGraph, OutboxDbContext dbContext)
            : base(resourceGraph, dbContext.Users)
        {
            _outboxMessageSet = dbContext.OutboxMessages;
        }

        public override Task OnWritingAsync(DomainUser user, OperationKind operationKind, CancellationToken cancellationToken)
        {
            return FinishWriteAsync(user, operationKind, cancellationToken);
        }

        protected override async Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            await _outboxMessageSet.AddAsync(message, cancellationToken);
        }
    }
}
