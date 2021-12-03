using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.TransactionalOutboxPattern
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class OutboxGroupDefinition : MessagingGroupDefinition
    {
        private readonly DbSet<OutgoingMessage> _outboxMessageSet;

        public OutboxGroupDefinition(IResourceGraph resourceGraph, OutboxDbContext dbContext, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, dbContext.Users, dbContext.Groups, hitCounter)
        {
            _outboxMessageSet = dbContext.OutboxMessages;
        }

        public override async Task OnWritingAsync(DomainGroup group, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            await base.OnWritingAsync(group, writeOperation, cancellationToken);

            await FinishWriteAsync(group, writeOperation, cancellationToken);
        }

        protected override async Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            await _outboxMessageSet.AddAsync(message, cancellationToken);
        }
    }
}
