using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations
{
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
        private readonly LocalIdValidator _localIdValidator;

        public OperationsProcessor(IOperationProcessorAccessor operationProcessorAccessor, IOperationsTransactionFactory operationsTransactionFactory,
            ILocalIdTracker localIdTracker, IResourceGraph resourceGraph, IJsonApiRequest request, ITargetedFields targetedFields)
        {
            ArgumentGuard.NotNull(operationProcessorAccessor, nameof(operationProcessorAccessor));
            ArgumentGuard.NotNull(operationsTransactionFactory, nameof(operationsTransactionFactory));
            ArgumentGuard.NotNull(localIdTracker, nameof(localIdTracker));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));

            _operationProcessorAccessor = operationProcessorAccessor;
            _operationsTransactionFactory = operationsTransactionFactory;
            _localIdTracker = localIdTracker;
            _resourceGraph = resourceGraph;
            _request = request;
            _targetedFields = targetedFields;
            _localIdValidator = new LocalIdValidator(_localIdTracker, _resourceGraph);
        }

        /// <inheritdoc />
        public virtual async Task<IList<OperationContainer>> ProcessAsync(IList<OperationContainer> operations, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(operations, nameof(operations));

            _localIdValidator.Validate(operations);
            _localIdTracker.Reset();

            var results = new List<OperationContainer>();

            await using IOperationsTransaction transaction = await _operationsTransactionFactory.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (OperationContainer operation in operations)
                {
                    operation.SetTransactionId(transaction.TransactionId);

                    await transaction.BeforeProcessOperationAsync(cancellationToken);

                    OperationContainer result = await ProcessOperationAsync(operation, cancellationToken);
                    results.Add(result);

                    await transaction.AfterProcessOperationAsync(cancellationToken);
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
                    error.Source.Pointer = $"/atomic:operations[{results.Count}]{error.Source.Pointer}";
                }

                throw;
            }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.InternalServerError)
                {
                    Title = "An unhandled error occurred while processing an operation in this request.",
                    Detail = exception.Message,
                    Source =
                    {
                        Pointer = $"/atomic:operations[{results.Count}]"
                    }
                }, exception);
            }

            return results;
        }

        protected virtual async Task<OperationContainer> ProcessOperationAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TrackLocalIdsForOperation(operation);

            _targetedFields.Attributes = operation.TargetedFields.Attributes;
            _targetedFields.Relationships = operation.TargetedFields.Relationships;

            _request.CopyFrom(operation.Request);

            return await _operationProcessorAccessor.ProcessAsync(operation, cancellationToken);
        }

        protected void TrackLocalIdsForOperation(OperationContainer operation)
        {
            if (operation.Kind == WriteOperationKind.CreateResource)
            {
                DeclareLocalId(operation.Resource);
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

        private void DeclareLocalId(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                ResourceContext resourceContext = _resourceGraph.GetResourceContext(resource.GetType());
                _localIdTracker.Declare(resource.LocalId, resourceContext.PublicName);
            }
        }

        private void AssignStringId(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                ResourceContext resourceContext = _resourceGraph.GetResourceContext(resource.GetType());
                resource.StringId = _localIdTracker.GetValue(resource.LocalId, resourceContext.PublicName);
            }
        }
    }
}
