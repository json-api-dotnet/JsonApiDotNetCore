using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <inheritdoc />
    public class OperationProcessorAccessor : IOperationProcessorAccessor
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IServiceProvider _serviceProvider;

        public OperationProcessorAccessor(IResourceContextProvider resourceContextProvider,
            IServiceProvider serviceProvider)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var processor = ResolveProcessor(operation);
            return processor.ProcessAsync(operation, cancellationToken);
        }

        protected virtual IOperationProcessor ResolveProcessor(OperationContainer operation)
        {
            var processorInterface = GetProcessorInterface(operation.Kind);
            var resourceContext = _resourceContextProvider.GetResourceContext(operation.Resource.GetType());

            var processorType = processorInterface.MakeGenericType(resourceContext.ResourceType, resourceContext.IdentityType);
            return (IOperationProcessor) _serviceProvider.GetRequiredService(processorType);
        }

        private static Type GetProcessorInterface(OperationKind kind)
        {
            switch (kind)
            {
                case OperationKind.CreateResource:
                    return typeof(ICreateProcessor<,>);
                case OperationKind.UpdateResource:
                    return typeof(IUpdateProcessor<,>);
                case OperationKind.DeleteResource:
                    return typeof(IDeleteProcessor<,>);
                case OperationKind.SetRelationship:
                    return typeof(ISetRelationshipProcessor<,>);
                case OperationKind.AddToRelationship:
                    return typeof(IAddToRelationshipProcessor<,>);
                case OperationKind.RemoveFromRelationship:
                    return typeof(IRemoveFromRelationshipProcessor<,>);
                default:
                    throw new NotSupportedException($"Unknown operation kind '{kind}'.");
            }
        }
    }
}
