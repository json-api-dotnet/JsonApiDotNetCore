using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Retrieves a <see cref="IOperationProcessor"/> instance from the D/I container and invokes a method on it.
    /// </summary>
    public interface IOperationProcessorAccessor
    {
        /// <summary>
        /// Invokes <see cref="IOperationProcessor.ProcessAsync"/> on a processor compatible with the operation kind.
        /// </summary>
        Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken);
    }
}
