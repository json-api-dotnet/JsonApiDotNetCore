using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single entry in a list of operations.
    /// </summary>
    public interface IAtomicOperationProcessor
    {
        Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken);
    }
}
