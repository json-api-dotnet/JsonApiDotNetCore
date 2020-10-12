using System;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// The error that is thrown when the underlying data store is unable to persist changes.
    /// </summary>
    public sealed class DataStorePersistFailedException : Exception
    {
        public DataStorePersistFailedException(Exception exception) 
            : base("Failed to persist changes in the underlying data store.", exception) { }
    }
}
