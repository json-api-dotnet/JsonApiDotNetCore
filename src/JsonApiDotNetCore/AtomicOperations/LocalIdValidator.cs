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
        private readonly IResourceContextProvider _resourceContextProvider;

        public LocalIdValidator(ILocalIdTracker localIdTracker, IResourceContextProvider resourceContextProvider)
        {
            ArgumentGuard.NotNull(localIdTracker, nameof(localIdTracker));
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));

            _localIdTracker = localIdTracker;
            _resourceContextProvider = resourceContextProvider;
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
                foreach (Error error in exception.Errors)
                {
                    error.Source.Pointer = $"/atomic:operations[{operationIndex}]" + error.Source.Pointer;
                }

                throw;
            }
        }

        private void ValidateOperation(OperationContainer operation)
        {
            if (operation.Kind == OperationKind.CreateResource)
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

            if (operation.Kind == OperationKind.CreateResource)
            {
                AssignLocalId(operation);
            }
        }

        private void DeclareLocalId(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(resource.GetType());
                _localIdTracker.Declare(resource.LocalId, resourceContext.PublicName);
            }
        }

        private void AssignLocalId(OperationContainer operation)
        {
            if (operation.Resource.LocalId != null)
            {
                ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(operation.Resource.GetType());

                _localIdTracker.Assign(operation.Resource.LocalId, resourceContext.PublicName, "placeholder");
            }
        }

        private void AssertLocalIdIsAssigned(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(resource.GetType());
                _localIdTracker.GetValue(resource.LocalId, resourceContext.PublicName);
            }
        }
    }
}
