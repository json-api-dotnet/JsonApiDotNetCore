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
        private readonly ResourceDefinitionHitCounter _hitCounter;
        private readonly DbSet<OutgoingMessage> _outboxMessageSet;

        public OutboxGroupDefinition(IResourceGraph resourceGraph, OutboxDbContext dbContext, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, dbContext.Users, dbContext.Groups, hitCounter)
        {
            _hitCounter = hitCounter;
            _outboxMessageSet = dbContext.OutboxMessages;
        }

        public override Task OnWritingAsync(DomainGroup group, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            _hitCounter.TrackInvocation<DomainGroup>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnWritingAsync);

            return FinishWriteAsync(group, writeOperation, cancellationToken);
        }

        protected override async Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            await _outboxMessageSet.AddAsync(message, cancellationToken);
        }
    }
}
