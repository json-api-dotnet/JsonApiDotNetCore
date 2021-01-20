using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Used to resolve a compatible <see cref="IOperationProcessor"/> at runtime, based on the operation kind.
    /// </summary>
    public interface IOperationProcessorResolver
    {
        /// <summary>
        /// Resolves a compatible <see cref="IOperationProcessor"/>.
        /// </summary>
        IOperationProcessor ResolveProcessor(OperationContainer operation);
    }
}
