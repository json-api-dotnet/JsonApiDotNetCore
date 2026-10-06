using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations;

/// <inheritdoc cref="IOperationsProcessor" />
[PublicAPI]
public class OperationsProcessor : IOperationsProcessor
{
    private readonly IOperationProcessorAccessor _operationProcessorAccessor;
    private readonly IOperationsTransactionFactory _operationsTransactionFactory;
    private readonly ILocalIdTracker _localIdTracker;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiRequest _request;
    private readonly ITargetedFields _targetedFields;
    private readonly ISparseFieldSetCache _sparseFieldSetCache;
    private readonly LocalIdValidator _localIdValidator;

    public OperationsProcessor(IOperationProcessorAccessor operationProcessorAccessor, IOperationsTransactionFactory operationsTransactionFactory,
        ILocalIdTracker localIdTracker, IResourceGraph resourceGraph, IJsonApiRequest request, ITargetedFields targetedFields,
        ISparseFieldSetCache sparseFieldSetCache)
    {
        ArgumentNullException.ThrowIfNull(operationProcessorAccessor);
        ArgumentNullException.ThrowIfNull(operationsTransactionFactory);
        ArgumentNullException.ThrowIfNull(localIdTracker);
        ArgumentNullException.ThrowIfNull(resourceGraph);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(targetedFields);
        ArgumentNullException.ThrowIfNull(sparseFieldSetCache);

        _operationProcessorAccessor = operationProcessorAccessor;
        _operationsTransactionFactory = operationsTransactionFactory;
        _localIdTracker = localIdTracker;
        _resourceGraph = resourceGraph;
        _request = request;
        _targetedFields = targetedFields;
        _sparseFieldSetCache = sparseFieldSetCache;
        _localIdValidator = new LocalIdValidator(_localIdTracker, _resourceGraph);
    }

    /// <inheritdoc />
    public virtual async Task<IList<OperationContainer?>> ProcessAsync(IList<OperationContainer> operations, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operations);

        _localIdValidator.Validate(operations);
        var indexTracker = new OperationIndexTracker();

        try
        {
            return await _operationsTransactionFactory.RunInTransactionAsync(async transaction =>
            {
                indexTracker.Reset();
                _localIdTracker.Reset();
                using IDisposable _ = new RevertRequestStateOnDispose(_request, _targetedFields);

                List<OperationContainer?> results = [];

                foreach (OperationContainer operation in operations)
                {
                    indexTracker.Increment();
                    operation.SetTransactionId(transaction.TransactionId);

                    await transaction.BeforeProcessOperationAsync(cancellationToken);

                    OperationContainer? result = await ProcessOperationAsync(operation, cancellationToken);
                    results.Add(result);

                    await transaction.AfterProcessOperationAsync(cancellationToken);

                    _sparseFieldSetCache.Reset();
                }

                indexTracker.Reset();
                return results;
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonApiException exception)
        {
            string operationPointer = indexTracker.OperationIndex != null ? $"/atomic:operations[{indexTracker.OperationIndex}]" : "/atomic:operations";

            foreach (ErrorObject error in exception.Errors)
            {
                error.Source ??= new ErrorSource();
                error.Source.Pointer = indexTracker.OperationIndex != null ? $"{operationPointer}{error.Source.Pointer}" : operationPointer;
            }

            throw;
        }
        catch (Exception exception)
        {
            throw new FailedOperationException(indexTracker.OperationIndex, exception);
        }
    }

    protected virtual async Task<OperationContainer?> ProcessOperationAsync(OperationContainer operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        cancellationToken.ThrowIfCancellationRequested();

        TrackLocalIdsForOperation(operation);

        _targetedFields.CopyFrom(operation.TargetedFields);
        _request.CopyFrom(operation.Request);

        return await _operationProcessorAccessor.ProcessAsync(operation, cancellationToken);
    }

    protected void TrackLocalIdsForOperation(OperationContainer operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (operation.Request.WriteOperation == WriteOperationKind.CreateResource)
        {
            DeclareLocalId(operation.Resource, operation.Request.PrimaryResourceType!);
        }
        else
        {
            AssignStringId(operation.Resource);
        }

        foreach (IIdentifiable secondaryResource in operation.GetSecondaryResources())
        {
            AssignStringId(secondaryResource);
        }
    }

    private void DeclareLocalId(IIdentifiable resource, ResourceType resourceType)
    {
        if (resource.LocalId != null)
        {
            _localIdTracker.Declare(resource.LocalId, resourceType);
        }
    }

    private void AssignStringId(IIdentifiable resource)
    {
        if (resource.LocalId != null)
        {
            ResourceType resourceType = _resourceGraph.GetResourceType(resource.GetClrType());
            resource.StringId = _localIdTracker.GetValue(resource.LocalId, resourceType);
        }
    }

    private sealed class OperationIndexTracker
    {
        public int? OperationIndex { get; private set; }

        public void Reset()
        {
            OperationIndex = null;
        }

        public void Increment()
        {
            if (OperationIndex == null)
            {
                OperationIndex = 0;
            }
            else
            {
                OperationIndex++;
            }
        }
    }
}
