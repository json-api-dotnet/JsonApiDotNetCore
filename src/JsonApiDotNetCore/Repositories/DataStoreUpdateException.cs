using System;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// The error that is thrown when the underlying data store is unable to persist changes.
    /// </summary>
    public sealed class DataStoreUpdateException : Exception
    {
        public DataStoreUpdateException(Exception exception)
            : base("Failed to persist changes in the underlying data store.", exception)
        {
        }
    }
}
