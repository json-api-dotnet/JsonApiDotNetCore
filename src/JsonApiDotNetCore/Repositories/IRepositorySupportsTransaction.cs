#nullable disable

using JetBrains.Annotations;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Used to indicate that an <see cref="IResourceRepository{TResource, TId}" /> supports execution inside a transaction.
    /// </summary>
    [PublicAPI]
    public interface IRepositorySupportsTransaction
    {
        /// <summary>
        /// Identifies the currently active transaction.
        /// </summary>
        string TransactionId { get; }
    }
}
