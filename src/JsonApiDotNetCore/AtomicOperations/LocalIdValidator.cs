using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Validates declaration, assignment and reference of local IDs within a list of operations.
    /// </summary>
    [PublicAPI]
    public sealed class LocalIdValidator
    {
        private readonly ILocalIdTracker _localIdTracker;
        private readonly IResourceGraph _resourceGraph;

        public LocalIdValidator(ILocalIdTracker localIdTracker, IResourceGraph resourceGraph)
        {
            ArgumentGuard.NotNull(localIdTracker, nameof(localIdTracker));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            _localIdTracker = localIdTracker;
            _resourceGraph = resourceGraph;
        }

        public void Validate(IEnumerable<OperationContainer> operations)
        {
            ArgumentGuard.NotNull(operations, nameof(operations));

            _localIdTracker.Reset();

            int operationIndex = 0;

            try
            {
                foreach (OperationContainer operation in operations)
                {
                    ValidateOperation(operation);

                    operationIndex++;
                }
            }
            catch (JsonApiException exception)
            {
                foreach (ErrorObject error in exception.Errors)
                {
                    error.Source ??= new ErrorSource();
                    error.Source.Pointer = $"/atomic:operations[{operationIndex}]{error.Source.Pointer}";
                }

                throw;
            }
        }

        private void ValidateOperation(OperationContainer operation)
        {
            if (operation.Kind == WriteOperationKind.CreateResource)
            {
                DeclareLocalId(operation.Resource);
            }
            else
            {
                AssertLocalIdIsAssigned(operation.Resource);
            }

            foreach (IIdentifiable secondaryResource in operation.GetSecondaryResources())
            {
                AssertLocalIdIsAssigned(secondaryResource);
            }

            if (operation.Kind == WriteOperationKind.CreateResource)
            {
                AssignLocalId(operation);
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

        private void AssignLocalId(OperationContainer operation)
        {
            if (operation.Resource.LocalId != null)
            {
                ResourceContext resourceContext = _resourceGraph.GetResourceContext(operation.Resource.GetType());
                _localIdTracker.Assign(operation.Resource.LocalId, resourceContext.PublicName, "placeholder");
            }
        }

        private void AssertLocalIdIsAssigned(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                ResourceContext resourceContext = _resourceGraph.GetResourceContext(resource.GetType());
                _localIdTracker.GetValue(resource.LocalId, resourceContext.PublicName);
            }
        }
    }
}
