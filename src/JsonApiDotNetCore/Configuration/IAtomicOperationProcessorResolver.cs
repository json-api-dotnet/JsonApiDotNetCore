using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Used to resolve the registered <see cref="IAtomicOperationProcessor"/> at runtime, based on the resource type in the operation.
    /// </summary>
    public interface IAtomicOperationProcessorResolver
    {
        /// <summary>
        /// Resolves a compatible <see cref="ICreateOperationProcessor{TResource}"/>.
        /// </summary>
        IAtomicOperationProcessor ResolveCreateProcessor(AtomicOperationObject operation);

        /// <summary>
        /// Resolves a compatible <see cref="IRemoveOperationProcessor{TResource, TId}"/>.
        /// </summary>
        IAtomicOperationProcessor ResolveRemoveProcessor(AtomicOperationObject operation);

        /// <summary>
        /// Resolves a compatible <see cref="IUpdateOperationProcessor{TResource, TId}"/>.
        /// </summary>
        IAtomicOperationProcessor ResolveUpdateProcessor(AtomicOperationObject operation);
    }
}
