using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Used to resolve a compatible <see cref="IAtomicOperationProcessor"/> at runtime, based on the operation code.
    /// </summary>
    public interface IAtomicOperationProcessorResolver
    {
        /// <summary>
        /// Resolves a compatible <see cref="IAtomicOperationProcessor"/>.
        /// </summary>
        IAtomicOperationProcessor ResolveProcessor(AtomicOperationObject operation);
    }
}
