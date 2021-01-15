using System;
using System.Threading;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// A transaction factory that throws when used in an atomic:operations request, because no transaction support is available.
    /// </summary>
    public sealed class MissingTransactionFactory : IAtomicOperationsTransactionFactory
    {
        public Task<IAtomicOperationsTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException("No transaction support is available.");
        }
    }
}
