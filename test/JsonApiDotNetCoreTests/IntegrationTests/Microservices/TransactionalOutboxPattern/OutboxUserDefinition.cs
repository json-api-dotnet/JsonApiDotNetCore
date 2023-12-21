using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.TransactionalOutboxPattern;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class OutboxUserDefinition(IResourceGraph resourceGraph, OutboxDbContext dbContext, ResourceDefinitionHitCounter hitCounter)
    : MessagingUserDefinition(resourceGraph, dbContext.Users, hitCounter)
{
    private readonly DbSet<OutgoingMessage> _outboxMessageSet = dbContext.OutboxMessages;

    public override async Task OnWritingAsync(DomainUser user, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        await base.OnWritingAsync(user, writeOperation, cancellationToken);

        await FinishWriteAsync(user, writeOperation, cancellationToken);
    }

    protected override async Task FlushMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
    {
        await _outboxMessageSet.AddAsync(message, cancellationToken);
    }
}
