using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.RequestBody;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class WorkflowDefinition(IResourceGraph resourceGraph) : JsonApiResourceDefinition<Workflow, Guid>(resourceGraph)
{
    private static readonly Dictionary<WorkflowStage, WorkflowStage[]> StageTransitionTable = new()
    {
        // @formatter:place_simple_list_pattern_on_single_line false

        [WorkflowStage.Created] =
        [
            WorkflowStage.InProgress
        ],
        [WorkflowStage.InProgress] =
        [
            WorkflowStage.OnHold,
            WorkflowStage.Succeeded,
            WorkflowStage.Failed,
            WorkflowStage.Canceled
        ],
        [WorkflowStage.OnHold] =
        [
            WorkflowStage.InProgress,
            WorkflowStage.Canceled
        ]

        // @formatter:place_simple_list_pattern_on_single_line restore
    };

    private WorkflowStage _previousStage;

    public override Task OnPrepareWriteAsync(Workflow resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (writeOperation == WriteOperationKind.UpdateResource)
        {
            _previousStage = resource.Stage;
        }

        return Task.CompletedTask;
    }

    public override Task OnWritingAsync(Workflow resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (writeOperation == WriteOperationKind.CreateResource)
        {
            AssertHasValidInitialStage(resource);
        }
        else if (writeOperation == WriteOperationKind.UpdateResource && resource.Stage != _previousStage)
        {
            AssertCanTransitionToStage(_previousStage, resource.Stage);
        }

        return Task.CompletedTask;
    }

    [AssertionMethod]
    private static void AssertHasValidInitialStage(Workflow resource)
    {
        if (resource.Stage != WorkflowStage.Created)
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.UnprocessableEntity)
            {
                Title = "Invalid workflow stage.",
                Detail = $"Initial stage of workflow must be '{WorkflowStage.Created}'.",
                Source = new ErrorSource
                {
                    Pointer = "/data/attributes/stage"
                }
            });
        }
    }

    [AssertionMethod]
    private static void AssertCanTransitionToStage(WorkflowStage fromStage, WorkflowStage toStage)
    {
        if (!CanTransitionToStage(fromStage, toStage))
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.UnprocessableEntity)
            {
                Title = "Invalid workflow stage.",
                Detail = $"Cannot transition from '{fromStage}' to '{toStage}'.",
                Source = new ErrorSource
                {
                    Pointer = "/data/attributes/stage"
                }
            });
        }
    }

    private static bool CanTransitionToStage(WorkflowStage fromStage, WorkflowStage toStage)
    {
        if (StageTransitionTable.TryGetValue(fromStage, out WorkflowStage[]? possibleNextStages))
        {
            return possibleNextStages.Contains(toStage);
        }

        return false;
    }
}
