using System;
using System.Threading;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// A transaction factory that throws when used in an atomic:operations request, because no transaction support is available.
    /// </summary>
    public sealed class MissingTransactionFactory : IOperationsTransactionFactory
    {
        /// <inheritdoc />
        public Task<IOperationsTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            // When using a data store other than Entity Framework Core, replace this type with your custom implementation
            // by overwriting the IoC container registration.
            throw new NotImplementedException("No transaction support is available.");
        }
    }
}
