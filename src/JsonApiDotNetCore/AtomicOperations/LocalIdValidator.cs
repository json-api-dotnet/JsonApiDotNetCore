using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Validates declaration, assignment and reference of local IDs within a list of operations.
    /// </summary>
    public sealed class LocalIdValidator
    {
        private readonly ILocalIdTracker _localIdTracker;
        private readonly IResourceContextProvider _resourceContextProvider;

        public LocalIdValidator(ILocalIdTracker localIdTracker, IResourceContextProvider resourceContextProvider)
        {
            _localIdTracker = localIdTracker ?? throw new ArgumentNullException(nameof(localIdTracker));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
        }

        public void Validate(IEnumerable<OperationContainer> operations)
        {
            _localIdTracker.Reset();

            int operationIndex = 0;

            try
            {
                foreach (var operation in operations)
                {
                    if (operation.Kind == OperationKind.CreateResource)
                    {
                        DeclareLocalId(operation.Resource);
                    }
                    else
                    {
                        AssertLocalIdIsAssigned(operation.Resource);
                    }

                    foreach (var secondaryResource in operation.GetSecondaryResources())
                    {
                        AssertLocalIdIsAssigned(secondaryResource);
                    }

                    if (operation.Kind == OperationKind.CreateResource)
                    {
                        AssignLocalId(operation);
                    }

                    operationIndex++;
                }

            }
            catch (JsonApiException exception)
            {
                foreach (var error in exception.Errors)
                {
                    error.Source.Pointer = $"/atomic:operations[{operationIndex}]" + error.Source.Pointer;
                }

                throw;
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

        private void AssignLocalId(OperationContainer operation)
        {
            if (operation.Resource.LocalId != null)
            {
                var resourceContext =
                    _resourceContextProvider.GetResourceContext(operation.Resource.GetType());

                _localIdTracker.Assign(operation.Resource.LocalId, resourceContext.PublicName, string.Empty);
            }
        }

        private void AssertLocalIdIsAssigned(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                var resourceContext = _resourceContextProvider.GetResourceContext(resource.GetType());
                _localIdTracker.GetValue(resource.LocalId, resourceContext.PublicName);
            }
        }
    }
}
