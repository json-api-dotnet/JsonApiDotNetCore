using System;
using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    public class OperationProcessorResolver : IOperationProcessorResolver
    {
        private readonly IGenericServiceFactory _genericServiceFactory;
        private readonly IResourceContextProvider _resourceContextProvider;

        public OperationProcessorResolver(IGenericServiceFactory genericServiceFactory,
            IResourceContextProvider resourceContextProvider)
        {
            _genericServiceFactory = genericServiceFactory ?? throw new ArgumentNullException(nameof(genericServiceFactory));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
        }

        /// <inheritdoc />
        public IOperationProcessor ResolveProcessor(OperationContainer operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            switch (operation.Kind)
            {
                case OperationKind.CreateResource:
                    return Resolve(operation, typeof(ICreateProcessor<,>));
                case OperationKind.UpdateResource:
                    return Resolve(operation, typeof(IUpdateProcessor<,>));
                case OperationKind.DeleteResource:
                    return Resolve(operation, typeof(IDeleteProcessor<,>));
                case OperationKind.SetRelationship:
                    return Resolve(operation, typeof(ISetRelationshipProcessor<,>));
                case OperationKind.AddToRelationship:
                    return Resolve(operation, typeof(IAddToRelationshipProcessor<,>));
                case OperationKind.RemoveFromRelationship:
                    return Resolve(operation, typeof(IRemoveFromRelationshipProcessor<,>));
                default:
                    throw new NotSupportedException($"Unknown operation kind '{operation.Kind}'.");
            }
        }

        private IOperationProcessor Resolve(OperationContainer operation, Type processorInterface)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext(operation.Resource.GetType());

            return _genericServiceFactory.Get<IOperationProcessor>(processorInterface,
                resourceContext.ResourceType, resourceContext.IdentityType
            );
        }
    }
}
