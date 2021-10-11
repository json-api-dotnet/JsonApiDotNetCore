#nullable disable

using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <summary>
    /// Validates and converts a 'ref' element in an entry of an atomic:operations request. It appears in most kinds of operations and typically indicates
    /// what would otherwise have been in the endpoint URL, if it were a resource request.
    /// </summary>
    public interface IAtomicReferenceAdapter
    {
        /// <summary>
        /// Validates and converts the specified <paramref name="atomicReference" />.
        /// </summary>
        AtomicReferenceResult Convert(AtomicReference atomicReference, ResourceIdentityRequirements requirements, RequestAdapterState state);
    }
}
