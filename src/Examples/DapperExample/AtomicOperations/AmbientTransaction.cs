using System.Data.Common;
using JsonApiDotNetCore.AtomicOperations;

namespace DapperExample.AtomicOperations;

/// <summary>
/// Represents an ADO.NET transaction in a JSON:API atomic:operations request.
/// </summary>
internal sealed class AmbientTransaction : IOperationsTransaction
{
    private readonly AmbientTransactionFactory _owner;

    public DbTransaction Current { get; }

    /// <inheritdoc />
    public string TransactionId { get; }

    public AmbientTransaction(AmbientTransactionFactory owner, DbTransaction current, Guid transactionId)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(current);

        _owner = owner;
        Current = current;
        TransactionId = transactionId.ToString();
    }

    /// <inheritdoc />
    public Task BeforeProcessOperationAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AfterProcessOperationAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return Current.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        DbConnection? connection = Current.Connection;

        await Current.DisposeAsync();

        if (connection != null)
        {
            await connection.DisposeAsync();
        }

        _owner.Detach(this);
    }
}
