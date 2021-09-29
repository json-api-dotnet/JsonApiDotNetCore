using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <inheritdoc />
    public sealed class OperationsDocumentAdapter : IOperationsDocumentAdapter
    {
        private readonly IJsonApiOptions _options;
        private readonly IAtomicOperationObjectAdapter _atomicOperationObjectAdapter;

        public OperationsDocumentAdapter(IJsonApiOptions options, IAtomicOperationObjectAdapter atomicOperationObjectAdapter)
        {
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(atomicOperationObjectAdapter, nameof(atomicOperationObjectAdapter));

            _options = options;
            _atomicOperationObjectAdapter = atomicOperationObjectAdapter;
        }

        /// <inheritdoc />
        public IList<OperationContainer> Convert(Document document, RequestAdapterState state)
        {
            ArgumentGuard.NotNull(state, nameof(state));
            AssertHasOperations(document.Operations, state);

            using (state.Position.PushElement("atomic:operations"))
            {
                AssertMaxOperationsNotExceeded(document.Operations, state);

                return ConvertOperations(document.Operations, state);
            }
        }

        private static void AssertHasOperations(IEnumerable<AtomicOperationObject> atomicOperationObjects, RequestAdapterState state)
        {
            if (atomicOperationObjects.IsNullOrEmpty())
            {
                throw new DeserializationException(state.Position, "No operations found.", null);
            }
        }

        private void AssertMaxOperationsNotExceeded(ICollection<AtomicOperationObject> atomicOperationObjects, RequestAdapterState state)
        {
            if (atomicOperationObjects.Count > _options.MaximumOperationsPerRequest)
            {
                throw new DeserializationException(state.Position, "Request exceeds the maximum number of operations.",
                    $"The number of operations in this request ({atomicOperationObjects.Count}) is higher than {_options.MaximumOperationsPerRequest}.");
            }
        }

        private IList<OperationContainer> ConvertOperations(IEnumerable<AtomicOperationObject> atomicOperationObjects, RequestAdapterState state)
        {
            var operations = new List<OperationContainer>();
            int operationIndex = 0;

            foreach (AtomicOperationObject atomicOperationObject in atomicOperationObjects)
            {
                using (state.Position.PushArrayIndex(operationIndex))
                {
                    OperationContainer operation = _atomicOperationObjectAdapter.Convert(atomicOperationObject, state);
                    operations.Add(operation);

                    operationIndex++;
                }
            }

            return operations;
        }
    }
}
