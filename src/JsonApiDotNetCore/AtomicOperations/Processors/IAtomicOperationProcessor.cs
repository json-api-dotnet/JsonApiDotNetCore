using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single entry in a list of atomic operations.
    /// </summary>
    public interface IAtomicOperationProcessor
    {
        Task<AtomicResultObject> ProcessAsync(AtomicOperationObject operation, CancellationToken cancellationToken);
    }
}
