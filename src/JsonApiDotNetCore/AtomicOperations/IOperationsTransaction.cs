using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Represents the overarching transaction in an atomic:operations request.
    /// </summary>
    [PublicAPI]
    public interface IOperationsTransaction : IAsyncDisposable
    {
        /// <summary>
        /// Identifies the active transaction.
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        /// Enables to execute custom logic before processing of an operation starts.
        /// </summary>
        Task BeforeProcessOperationAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Enables to execute custom logic after processing of an operation succeeds.
        /// </summary>
        Task AfterProcessOperationAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Commits all changes made to the underlying data store.
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken);
    }
}
