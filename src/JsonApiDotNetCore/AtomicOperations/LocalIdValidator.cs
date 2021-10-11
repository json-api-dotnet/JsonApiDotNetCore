#nullable disable

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
            if (operation.Request.WriteOperation == WriteOperationKind.CreateResource)
            {
                DeclareLocalId(operation.Resource, operation.Request.PrimaryResourceType);
            }
            else
            {
                AssertLocalIdIsAssigned(operation.Resource);
            }

            foreach (IIdentifiable secondaryResource in operation.GetSecondaryResources())
            {
                AssertLocalIdIsAssigned(secondaryResource);
            }

            if (operation.Request.WriteOperation == WriteOperationKind.CreateResource)
            {
                AssignLocalId(operation, operation.Request.PrimaryResourceType);
            }
        }

        private void DeclareLocalId(IIdentifiable resource, ResourceType resourceType)
        {
            if (resource.LocalId != null)
            {
                _localIdTracker.Declare(resource.LocalId, resourceType.PublicName);
            }
        }

        private void AssignLocalId(OperationContainer operation, ResourceType resourceType)
        {
            if (operation.Resource.LocalId != null)
            {
                _localIdTracker.Assign(operation.Resource.LocalId, resourceType.PublicName, "placeholder");
            }
        }

        private void AssertLocalIdIsAssigned(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                ResourceType resourceType = _resourceGraph.GetResourceType(resource.GetType());
                _localIdTracker.GetValue(resource.LocalId, resourceType.PublicName);
            }
        }
    }
}
