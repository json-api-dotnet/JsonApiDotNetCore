using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Used to resolve a compatible <see cref="IAtomicOperationProcessor"/> at runtime, based on the operation kind.
    /// </summary>
    public interface IAtomicOperationProcessorResolver
    {
        /// <summary>
        /// Resolves a compatible <see cref="IAtomicOperationProcessor"/>.
        /// </summary>
        IAtomicOperationProcessor ResolveProcessor(OperationContainer operation);
    }
}
