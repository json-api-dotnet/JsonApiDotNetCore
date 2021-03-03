using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <inheritdoc />
    [PublicAPI]
    public class OperationProcessorAccessor : IOperationProcessorAccessor
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IServiceProvider _serviceProvider;

        public OperationProcessorAccessor(IResourceContextProvider resourceContextProvider, IServiceProvider serviceProvider)
        {
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));
            ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));

            _resourceContextProvider = resourceContextProvider;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(operation, nameof(operation));

            IOperationProcessor processor = ResolveProcessor(operation);
            return processor.ProcessAsync(operation, cancellationToken);
        }

        protected virtual IOperationProcessor ResolveProcessor(OperationContainer operation)
        {
            Type processorInterface = GetProcessorInterface(operation.Kind);
            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(operation.Resource.GetType());

            Type processorType = processorInterface.MakeGenericType(resourceContext.ResourceType, resourceContext.IdentityType);
            return (IOperationProcessor)_serviceProvider.GetRequiredService(processorType);
        }

        private static Type GetProcessorInterface(OperationKind kind)
        {
            switch (kind)
            {
                case OperationKind.CreateResource:
                {
                    return typeof(ICreateProcessor<,>);
                }
                case OperationKind.UpdateResource:
                {
                    return typeof(IUpdateProcessor<,>);
                }
                case OperationKind.DeleteResource:
                {
                    return typeof(IDeleteProcessor<,>);
                }
                case OperationKind.SetRelationship:
                {
                    return typeof(ISetRelationshipProcessor<,>);
                }
                case OperationKind.AddToRelationship:
                {
                    return typeof(IAddToRelationshipProcessor<,>);
                }
                case OperationKind.RemoveFromRelationship:
                {
                    return typeof(IRemoveFromRelationshipProcessor<,>);
                }
                default:
                {
                    throw new NotSupportedException($"Unknown operation kind '{kind}'.");
                }
            }
        }
    }
}
