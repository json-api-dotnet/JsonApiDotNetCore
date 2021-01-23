using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <inheritdoc />
    public class OperationsProcessor : IOperationsProcessor
    {
        private readonly IOperationProcessorAccessor _operationProcessorAccessor;
        private readonly IOperationsTransactionFactory _operationsTransactionFactory;
        private readonly ILocalIdTracker _localIdTracker;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IJsonApiRequest _request;
        private readonly ITargetedFields _targetedFields;
        private readonly LocalIdValidator _localIdValidator;

        public OperationsProcessor(IOperationProcessorAccessor operationProcessorAccessor,
            IOperationsTransactionFactory operationsTransactionFactory, ILocalIdTracker localIdTracker,
            IResourceContextProvider resourceContextProvider, IJsonApiRequest request, ITargetedFields targetedFields)
        {
            _operationProcessorAccessor = operationProcessorAccessor ?? throw new ArgumentNullException(nameof(operationProcessorAccessor));
            _operationsTransactionFactory = operationsTransactionFactory ?? throw new ArgumentNullException(nameof(operationsTransactionFactory));
            _localIdTracker = localIdTracker ?? throw new ArgumentNullException(nameof(localIdTracker));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _localIdValidator = new LocalIdValidator(_localIdTracker, _resourceContextProvider);
        }

        /// <inheritdoc />
        public virtual async Task<IList<OperationContainer>> ProcessAsync(IList<OperationContainer> operations,
            CancellationToken cancellationToken)
        {
            if (operations == null) throw new ArgumentNullException(nameof(operations));

            _localIdValidator.Validate(operations);
            _localIdTracker.Reset();

            var results = new List<OperationContainer>();

            await using var transaction = await _operationsTransactionFactory.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var operation in operations)
                {
                    operation.SetTransactionId(transaction.TransactionId);

                    await transaction.BeforeProcessOperationAsync(cancellationToken);

                    var result = await ProcessOperation(operation, cancellationToken);
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
                foreach (var error in exception.Errors)
                {
                    error.Source.Pointer = $"/atomic:operations[{results.Count}]" + error.Source.Pointer;
                }

                throw;
            }
            catch (Exception exception)
            {
                throw new JsonApiException(new Error(HttpStatusCode.InternalServerError)
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

        protected virtual async Task<OperationContainer> ProcessOperation(OperationContainer operation,
            CancellationToken cancellationToken)
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
            if (operation.Kind == OperationKind.CreateResource)
            {
                DeclareLocalId(operation.Resource);
            }
            else
            {
                AssignStringId(operation.Resource);
            }

            foreach (var secondaryResource in operation.GetSecondaryResources())
            {
                AssignStringId(secondaryResource);
            }
        }

        private void DeclareLocalId(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                var resourceContext = _resourceContextProvider.GetResourceContext(resource.GetType());
                _localIdTracker.Declare(resource.LocalId, resourceContext.PublicName);
            }
        }

        private void AssignStringId(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                var resourceContext = _resourceContextProvider.GetResourceContext(resource.GetType());
                resource.StringId = _localIdTracker.GetValue(resource.LocalId, resourceContext.PublicName);
            }
        }
    }
}
