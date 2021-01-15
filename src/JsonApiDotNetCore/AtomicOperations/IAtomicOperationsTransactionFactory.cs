using System.Threading;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Provides a method to start the overarching transaction for an atomic:operations request.
    /// </summary>
    public interface IAtomicOperationsTransactionFactory
    {
        /// <summary>
        /// Starts a new transaction.
        /// </summary>
        Task<IAtomicOperationsTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}
