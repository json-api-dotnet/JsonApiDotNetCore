using System.Threading;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Provides a method to start the overarching transaction for an atomic:operations request.
    /// </summary>
    public interface IOperationsTransactionFactory
    {
        /// <summary>
        /// Starts a new transaction.
        /// </summary>
        Task<IOperationsTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}
