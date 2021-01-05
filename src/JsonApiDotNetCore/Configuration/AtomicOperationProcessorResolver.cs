using System;
using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    public class AtomicOperationProcessorResolver : IAtomicOperationProcessorResolver
    {
        private readonly IGenericServiceFactory _genericServiceFactory;
        private readonly IResourceContextProvider _resourceContextProvider;

        public AtomicOperationProcessorResolver(IGenericServiceFactory genericServiceFactory,
            IResourceContextProvider resourceContextProvider)
        {
            _genericServiceFactory = genericServiceFactory ?? throw new ArgumentNullException(nameof(genericServiceFactory));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
        }

        /// <inheritdoc />
        public IAtomicOperationProcessor ResolveProcessor(OperationContainer operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            // TODO: @OPS: How about processors with a single type argument?

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

        private IAtomicOperationProcessor Resolve(OperationContainer operation, Type processorInterface)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext(operation.Resource.GetType());

            return _genericServiceFactory.Get<IAtomicOperationProcessor>(processorInterface,
                resourceContext.ResourceType, resourceContext.IdentityType
            );
        }
    }
}
