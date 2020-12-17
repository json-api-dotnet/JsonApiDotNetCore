using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Atomically processes a request that contains a list of operations.
    /// </summary>
    public interface IAtomicOperationsProcessor
    {
        Task<IList<AtomicResultObject>> ProcessAsync(IList<AtomicOperationObject> operations, CancellationToken cancellationToken);
    }
}
