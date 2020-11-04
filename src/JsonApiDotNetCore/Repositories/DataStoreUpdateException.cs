using System;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// The error that is thrown when the underlying data store is unable to persist changes.
    /// </summary>
    public sealed class DataStoreUpdateException : Exception
    {
        // TODO: Add second overload with message.
        public DataStoreUpdateException(Exception exception = null) 
            : base("Failed to persist changes in the underlying data store.", exception) { }
    }
}
