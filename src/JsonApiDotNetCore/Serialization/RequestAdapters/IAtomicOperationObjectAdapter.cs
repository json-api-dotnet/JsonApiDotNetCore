using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <summary>
    /// Validates and converts a single operation inside an atomic:operations request.
    /// </summary>
    public interface IAtomicOperationObjectAdapter
    {
        /// <summary>
        /// Validates and converts the specified <paramref name="atomicOperationObject" />.
        /// </summary>
        OperationContainer Convert(AtomicOperationObject atomicOperationObject, RequestAdapterState state);
    }
}
