using JetBrains.Annotations;

namespace JsonApiDotNetCore.Repositories;

/// <summary>
/// The error that is thrown when the underlying data store is unable to persist changes.
/// </summary>
[PublicAPI]
public class DataStoreUpdateException : Exception
{
    public DataStoreUpdateException(Exception? innerException)
        : this("Failed to persist changes in the underlying data store.", innerException)
    {
    }

    protected DataStoreUpdateException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
