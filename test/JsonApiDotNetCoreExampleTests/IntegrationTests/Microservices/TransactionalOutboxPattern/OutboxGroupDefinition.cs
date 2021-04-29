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
    public sealed class OutboxGroupDefinition : MessagingGroupDefinition
    {
        private readonly DbSet<OutgoingMessage> _outboxMessageSet;

        public OutboxGroupDefinition(IResourceGraph resourceGraph, OutboxDbContext dbContext)
            : base(resourceGraph, dbContext.Users, dbContext.Groups)
        {
            _outboxMessageSet = dbContext.OutboxMessages;
        }

        public override Task OnWritingAsync(DomainGroup group, OperationKind operationKind, CancellationToken cancellationToken)
        {
            return FinishWriteAsync(group, operationKind, cancellationToken);
        }

        protected override async Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            await _outboxMessageSet.AddAsync(message, cancellationToken);
        }
    }
}
