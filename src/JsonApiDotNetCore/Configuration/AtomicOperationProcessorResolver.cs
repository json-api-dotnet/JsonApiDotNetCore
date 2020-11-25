using System;
using System.Net;
using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

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
        public IAtomicOperationProcessor ResolveProcessor(AtomicOperationObject operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            switch (operation.Code)
            {
             case AtomicOperationCode.Add:
                 return ResolveCreateProcessor(operation);
             case AtomicOperationCode.Remove:
                 return ResolveRemoveProcessor(operation);
             case AtomicOperationCode.Update:
                 return ResolveUpdateProcessor(operation);
            }

            throw new InvalidOperationException($"Atomic operation code '{operation.Code}' is invalid.");
        }

        private IAtomicOperationProcessor ResolveCreateProcessor(AtomicOperationObject operation)
        {
            return Resolve(operation, typeof(ICreateOperationProcessor<,>));
        }

        private IAtomicOperationProcessor ResolveRemoveProcessor(AtomicOperationObject operation)
        {
            return Resolve(operation, typeof(IRemoveOperationProcessor<,>));
        }

        private IAtomicOperationProcessor ResolveUpdateProcessor(AtomicOperationObject operation)
        {
            return Resolve(operation, typeof(IUpdateOperationProcessor<,>));
        }

        private IAtomicOperationProcessor Resolve(AtomicOperationObject atomicOperationObject, Type processorInterface)
        {
            var resourceName = atomicOperationObject.GetResourceTypeName();
            var resourceContext = GetResourceContext(resourceName);

            return _genericServiceFactory.Get<IAtomicOperationProcessor>(processorInterface,
                resourceContext.ResourceType, resourceContext.IdentityType
            );
        }

        private ResourceContext GetResourceContext(string resourceName)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext(resourceName);
            if (resourceContext == null)
            {
                // TODO: @OPS: Should have validated this earlier in the call stack.

                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Unsupported resource type.",
                    Detail = $"This API does not expose a resource of type '{resourceName}'."
                });
            }

            return resourceContext;
        }
    }
}
