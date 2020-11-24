using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Processes a request that contains a list of atomic operations.
    /// </summary>
    public interface IAtomicOperationsProcessor
    {
        Task<IList<AtomicOperation>> ProcessAsync(IList<AtomicOperation> operations, CancellationToken cancellationToken);
    }
}
