using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single entry in a list of operations.
    /// </summary>
    public interface IOperationProcessor
    {
        /// <summary>
        /// Processes the specified operation.
        /// </summary>
        Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken);
    }
}
