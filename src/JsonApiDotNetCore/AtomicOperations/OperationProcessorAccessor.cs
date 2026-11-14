using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.AtomicOperations;

/// <inheritdoc cref="IOperationProcessorAccessor" />
[PublicAPI]
public class OperationProcessorAccessor : IOperationProcessorAccessor
{
    private readonly IServiceProvider _serviceProvider;

    public OperationProcessorAccessor(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public Task<OperationContainer?> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        IOperationProcessor processor = ResolveProcessor(operation);
        return processor.ProcessAsync(operation, cancellationToken);
    }

    protected virtual IOperationProcessor ResolveProcessor(OperationContainer operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Type processorInterface = GetProcessorInterface(operation.Request.WriteOperation!.Value);
        ResourceType resourceType = operation.Request.PrimaryResourceType!;

        Type processorType = processorInterface.MakeGenericType(resourceType.ClrType, resourceType.IdentityClrType);
        return (IOperationProcessor)_serviceProvider.GetRequiredService(processorType);
    }

    private static Type GetProcessorInterface(WriteOperationKind writeOperation)
    {
        Type? processorInterface = null;

        switch (writeOperation)
        {
            case WriteOperationKind.CreateResource:
                processorInterface = typeof(ICreateProcessor<,>);
                break;
            case WriteOperationKind.UpdateResource:
                processorInterface = typeof(IUpdateProcessor<,>);
                break;
            case WriteOperationKind.DeleteResource:
                processorInterface = typeof(IDeleteProcessor<,>);
                break;
            case WriteOperationKind.SetRelationship:
                processorInterface = typeof(ISetRelationshipProcessor<,>);
                break;
            case WriteOperationKind.AddToRelationship:
                processorInterface = typeof(IAddToRelationshipProcessor<,>);
                break;
            case WriteOperationKind.RemoveFromRelationship:
                processorInterface = typeof(IRemoveFromRelationshipProcessor<,>);
                break;
        }

        ConsistencyGuard.ThrowIf(processorInterface == null, $"Unknown write operation kind '{writeOperation}'.");

        return processorInterface;
    }
}
