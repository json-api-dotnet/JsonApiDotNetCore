using System;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Used to indicate that a <see cref="IResourceRepository{TResource}"/> supports execution inside a transaction.
    /// </summary>
    public interface IRepositorySupportsTransaction
    {
        /// <summary>
        /// Identifies the currently active transaction.
        /// </summary>
        Guid? TransactionId { get; }
    }
}
