using System.Data.Common;
using DapperExample.TranslationToSql.DataModel;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;

namespace DapperExample.AtomicOperations;

/// <summary>
/// Provides transaction support for JSON:API atomic:operation requests using ADO.NET.
/// </summary>
public sealed class AmbientTransactionFactory : IOperationsTransactionFactory
{
    private readonly IJsonApiOptions _options;
    private readonly IDataModelService _dataModelService;

    internal AmbientTransaction? AmbientTransaction { get; private set; }

    public AmbientTransactionFactory(IJsonApiOptions options, IDataModelService dataModelService)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(dataModelService);

        _options = options;
        _dataModelService = dataModelService;
    }

    /// <inheritdoc />
    public async Task<TResult> RunInTransactionAsync<TResult>(Func<IOperationsTransaction, Task<TResult>> asyncAction, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(asyncAction);

        await using AmbientTransaction transaction = await BeginTransactionAsync(cancellationToken);

        TResult result = await asyncAction(transaction);

        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    private async Task<AmbientTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (AmbientTransaction != null)
        {
            throw new InvalidOperationException("Cannot start transaction because another transaction is already active.");
        }

        DbConnection dbConnection = _dataModelService.CreateConnection();

        try
        {
            await dbConnection.OpenAsync(cancellationToken);

            DbTransaction transaction = _options.TransactionIsolationLevel != null
                ? await dbConnection.BeginTransactionAsync(_options.TransactionIsolationLevel.Value, cancellationToken)
                : await dbConnection.BeginTransactionAsync(cancellationToken);

            var transactionId = Guid.NewGuid();
            AmbientTransaction = new AmbientTransaction(this, transaction, transactionId);

            return AmbientTransaction;
        }
        catch (DbException)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    internal void Detach(AmbientTransaction ambientTransaction)
    {
        ArgumentNullException.ThrowIfNull(ambientTransaction);

        if (AmbientTransaction != null && AmbientTransaction == ambientTransaction)
        {
            AmbientTransaction = null;
        }
        else
        {
            throw new InvalidOperationException("Failed to detach ambient transaction.");
        }
    }
}
