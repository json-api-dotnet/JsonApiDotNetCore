using System;
using System.Threading;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Represents the overarching transaction in an atomic:operations request.
    /// </summary>
    public interface IAtomicOperationsTransaction : IAsyncDisposable
    {
        /// <summary>
        /// Identifies the active transaction.
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        /// Enables to execute custom logic before processing of the next operation starts.
        /// </summary>
        void PrepareForNextOperation();

        /// <summary>
        /// Commits all changes made to the underlying data store.
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Discards all changes made to the underlying data store.
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken);
    }
}
