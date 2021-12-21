using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class LeafSignalingDefinition : JsonApiResourceDefinition<Leaf, long>
{
    internal const string WaitForResumeSignalHeaderName = "X-WaitForResumeSignal";

    private readonly TestExecutionMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LeafSignalingDefinition(IResourceGraph resourceGraph, TestExecutionMediator mediator, IHttpContextAccessor httpContextAccessor)
        : base(resourceGraph)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task OnPrepareWriteAsync(Leaf resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext!.Request.Headers.ContainsKey(WaitForResumeSignalHeaderName))
        {
            await _mediator.NotifyTransactionStartedAsync(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}
