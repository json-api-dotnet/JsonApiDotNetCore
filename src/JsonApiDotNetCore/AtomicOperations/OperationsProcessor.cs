using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations;

/// <inheritdoc />
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
        ArgumentGuard.NotNull(operationProcessorAccessor, nameof(operationProcessorAccessor));
        ArgumentGuard.NotNull(operationsTransactionFactory, nameof(operationsTransactionFactory));
        ArgumentGuard.NotNull(localIdTracker, nameof(localIdTracker));
        ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
        ArgumentGuard.NotNull(request, nameof(request));
        ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));
        ArgumentGuard.NotNull(sparseFieldSetCache, nameof(sparseFieldSetCache));

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
        ArgumentGuard.NotNull(operations, nameof(operations));

        _localIdValidator.Validate(operations);
        _localIdTracker.Reset();

        var results = new List<OperationContainer?>();

        await using IOperationsTransaction transaction = await _operationsTransactionFactory.BeginTransactionAsync(cancellationToken);

        try
        {
            using IDisposable _ = new RevertRequestStateOnDispose(_request, _targetedFields);

            foreach (OperationContainer operation in operations)
            {
                operation.SetTransactionId(transaction.TransactionId);

                await transaction.BeforeProcessOperationAsync(cancellationToken);

                OperationContainer? result = await ProcessOperationAsync(operation, cancellationToken);
                results.Add(result);

                await transaction.AfterProcessOperationAsync(cancellationToken);

                _sparseFieldSetCache.Reset();
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonApiException exception)
        {
            foreach (ErrorObject error in exception.Errors)
            {
                error.Source ??= new ErrorSource();
                error.Source.Pointer = $"/atomic:operations[{results.Count}]{error.Source.Pointer}";
            }

            throw;
        }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
        catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
        {
            throw new FailedOperationException(results.Count, exception);
        }

        return results;
    }

    protected virtual async Task<OperationContainer?> ProcessOperationAsync(OperationContainer operation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TrackLocalIdsForOperation(operation);

        _targetedFields.CopyFrom(operation.TargetedFields);
        _request.CopyFrom(operation.Request);

        return await _operationProcessorAccessor.ProcessAsync(operation, cancellationToken);
    }

    protected void TrackLocalIdsForOperation(OperationContainer operation)
    {
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
}
