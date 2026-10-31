using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

/// <summary>
/// Used to simulate side effects that occur in the database while saving, typically caused by database triggers.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ImplicitlyChangingWorkItemDefinition(IResourceGraph resourceGraph, ReadWriteDbContext dbContext)
    : JsonApiResourceDefinition<WorkItem, int>(resourceGraph)
{
    internal const string Suffix = " (changed)";

    /// <summary>
    /// When a work item has this description, only its estimate is changed.
    /// </summary>
    internal const string ChangeEstimateMarker = "change-estimate";

    /// <summary>
    /// When a work item has this description, no side effects occur.
    /// </summary>
    internal const string NoSideEffectsMarker = "no-side-effects";

    /// <summary>
    /// When a work item has this description, only its checklist (a compound collection) is changed.
    /// </summary>
    internal const string ChangeChecklistMarker = "change-checklist";

    /// <summary>
    /// When a work item has this description, only its estimate (a nullable compound) is cleared.
    /// </summary>
    internal const string ClearEstimateMarker = "clear-estimate";

    internal static readonly TimeSpan ChangedLikelyEstimate = TimeSpan.FromHours(8);

    private readonly ReadWriteDbContext _dbContext = dbContext;

    public override async Task OnWriteSucceededAsync(WorkItem resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (resource.Description == NoSideEffectsMarker)
        {
            return;
        }

        if (resource.Description == ChangeEstimateMarker)
        {
            if (writeOperation is not WriteOperationKind.DeleteResource)
            {
                const string json = """{"Optimistic":"01:00:00","Likely":"08:00:00","Pessimistic":"16:00:00"}""";
                const string statement = "UPDATE \"WorkItems\" SET \"Estimate\" = CAST({0} AS jsonb) WHERE \"Id\" = {1}";

                await _dbContext.Database.ExecuteSqlRawAsync(statement, [json, resource.Id], cancellationToken);
            }

            return;
        }

        if (resource.Description == ChangeChecklistMarker)
        {
            if (writeOperation is not WriteOperationKind.DeleteResource)
            {
                const string json = """[{"Text":"Changed by definition","IsDone":true}]""";
                const string statement = "UPDATE \"WorkItems\" SET \"Checklist\" = CAST({0} AS jsonb) WHERE \"Id\" = {1}";

                await _dbContext.Database.ExecuteSqlRawAsync(statement, [
                    json,
                    resource.Id
                ], cancellationToken);
            }

            return;
        }

        if (resource.Description == ClearEstimateMarker)
        {
            if (writeOperation is not WriteOperationKind.DeleteResource)
            {
                const string statement = "UPDATE \"WorkItems\" SET \"Estimate\" = NULL WHERE \"Id\" = {0}";
                await _dbContext.Database.ExecuteSqlRawAsync(statement, [resource.Id], cancellationToken);
            }

            return;
        }

        if (writeOperation is not WriteOperationKind.DeleteResource)
        {
            string statement = $"Update \"WorkItems\" SET \"Description\" = '{resource.Description}{Suffix}' WHERE \"Id\" = '{resource.StringId}'";
            await _dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }
}
