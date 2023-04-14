using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.AtomicOperations;

/// <inheritdoc />
[PublicAPI]
public class OperationProcessorAccessor : IOperationProcessorAccessor
{
    private readonly IServiceProvider _serviceProvider;

    public OperationProcessorAccessor(IServiceProvider serviceProvider)
    {
        ArgumentGuard.NotNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public Task<OperationContainer?> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(operation);

        IOperationProcessor processor = ResolveProcessor(operation);
        return processor.ProcessAsync(operation, cancellationToken);
    }

    protected virtual IOperationProcessor ResolveProcessor(OperationContainer operation)
    {
        Type processorInterface = GetProcessorInterface(operation.Request.WriteOperation!.Value);
        ResourceType resourceType = operation.Request.PrimaryResourceType!;

        Type processorType = processorInterface.MakeGenericType(resourceType.ClrType, resourceType.IdentityClrType);
        return (IOperationProcessor)_serviceProvider.GetRequiredService(processorType);
    }

    private static Type GetProcessorInterface(WriteOperationKind writeOperation)
    {
        return writeOperation switch
        {
            WriteOperationKind.CreateResource => typeof(ICreateProcessor<,>),
            WriteOperationKind.UpdateResource => typeof(IUpdateProcessor<,>),
            WriteOperationKind.DeleteResource => typeof(IDeleteProcessor<,>),
            WriteOperationKind.SetRelationship => typeof(ISetRelationshipProcessor<,>),
            WriteOperationKind.AddToRelationship => typeof(IAddToRelationshipProcessor<,>),
            WriteOperationKind.RemoveFromRelationship => typeof(IRemoveFromRelationshipProcessor<,>),
            _ => throw new NotSupportedException($"Unknown write operation kind '{writeOperation}'.")
        };
    }
}
