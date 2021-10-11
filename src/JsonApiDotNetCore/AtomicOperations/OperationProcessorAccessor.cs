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
        private readonly IServiceProvider _serviceProvider;

        public OperationProcessorAccessor(IServiceProvider serviceProvider)
        {
            ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public Task<OperationContainer?> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(operation, nameof(operation));

            IOperationProcessor processor = ResolveProcessor(operation);
            return processor.ProcessAsync(operation, cancellationToken);
        }

        protected virtual IOperationProcessor ResolveProcessor(OperationContainer operation)
        {
            Type processorInterface = GetProcessorInterface(operation.Request.WriteOperation.GetValueOrDefault());
            ResourceType resourceType = operation.Request.PrimaryResourceType!;

            Type processorType = processorInterface.MakeGenericType(resourceType.ClrType, resourceType.IdentityClrType);
            return (IOperationProcessor)_serviceProvider.GetRequiredService(processorType);
        }

        private static Type GetProcessorInterface(WriteOperationKind writeOperation)
        {
            switch (writeOperation)
            {
                case WriteOperationKind.CreateResource:
                {
                    return typeof(ICreateProcessor<,>);
                }
                case WriteOperationKind.UpdateResource:
                {
                    return typeof(IUpdateProcessor<,>);
                }
                case WriteOperationKind.DeleteResource:
                {
                    return typeof(IDeleteProcessor<,>);
                }
                case WriteOperationKind.SetRelationship:
                {
                    return typeof(ISetRelationshipProcessor<,>);
                }
                case WriteOperationKind.AddToRelationship:
                {
                    return typeof(IAddToRelationshipProcessor<,>);
                }
                case WriteOperationKind.RemoveFromRelationship:
                {
                    return typeof(IRemoveFromRelationshipProcessor<,>);
                }
                default:
                {
                    throw new NotSupportedException($"Unknown write operation kind '{writeOperation}'.");
                }
            }
        }
    }
}
