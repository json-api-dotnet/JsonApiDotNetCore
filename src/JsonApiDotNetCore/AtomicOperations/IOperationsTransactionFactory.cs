namespace JsonApiDotNetCore.AtomicOperations;

/// <summary>
/// Provides a method to execute operations within an overarching transaction for an atomic:operations request.
/// </summary>
public interface IOperationsTransactionFactory
{
    /// <summary>
    /// Executes the specified asynchronous callback within an overarching transaction.
    /// </summary>
    /// <typeparam name="TResult">
    /// The return type of the callback.
    /// </typeparam>
    /// <param name="asyncAction">
    /// The callback to execute within the transaction.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates notification that request handling should be canceled.
    /// </param>
    Task<TResult> RunInTransactionAsync<TResult>(Func<IOperationsTransaction, Task<TResult>> asyncAction, CancellationToken cancellationToken);
}
