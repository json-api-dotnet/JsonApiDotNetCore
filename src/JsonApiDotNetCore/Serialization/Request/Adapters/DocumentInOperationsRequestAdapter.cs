using System.Diagnostics.CodeAnalysis;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc cref="IDocumentInOperationsRequestAdapter" />
public sealed class DocumentInOperationsRequestAdapter : BaseAdapter, IDocumentInOperationsRequestAdapter
{
    private readonly IJsonApiOptions _options;
    private readonly IAtomicOperationObjectAdapter _atomicOperationObjectAdapter;

    public DocumentInOperationsRequestAdapter(IJsonApiOptions options, IAtomicOperationObjectAdapter atomicOperationObjectAdapter)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(atomicOperationObjectAdapter);

        _options = options;
        _atomicOperationObjectAdapter = atomicOperationObjectAdapter;
    }

    /// <inheritdoc />
    public IList<OperationContainer> Convert(Document document, RequestAdapterState state)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(state);

        AssertHasOperations(document.Operations, state);

        using IDisposable _ = state.Position.PushElement("atomic:operations");
        AssertMaxOperationsNotExceeded(document.Operations, state);

        return ConvertOperations(document.Operations, state);
    }

    private static void AssertHasOperations([NotNull] IEnumerable<AtomicOperationObject?>? atomicOperationObjects, RequestAdapterState state)
    {
        if (atomicOperationObjects.IsNullOrEmpty())
        {
            throw new ModelConversionException(state.Position, "No operations found.", null);
        }
    }

    private void AssertMaxOperationsNotExceeded(IList<AtomicOperationObject?> atomicOperationObjects, RequestAdapterState state)
    {
        if (atomicOperationObjects.Count > _options.MaximumOperationsPerRequest)
        {
            throw new ModelConversionException(state.Position, "Too many operations in request.",
                $"The number of operations in this request ({atomicOperationObjects.Count}) is higher than the maximum of {_options.MaximumOperationsPerRequest}.");
        }
    }

    private List<OperationContainer> ConvertOperations(IEnumerable<AtomicOperationObject?> atomicOperationObjects, RequestAdapterState state)
    {
        List<OperationContainer> operations = [];

        foreach (AtomicOperationObject? atomicOperationObject in atomicOperationObjects)
        {
            using IDisposable _ = state.Position.PushArrayIndex(operations.Count);
            AssertObjectIsNotNull(atomicOperationObject, state);

            OperationContainer operation = _atomicOperationObjectAdapter.Convert(atomicOperationObject, state);
            operations.Add(operation);
        }

        return operations;
    }
}
