using JetBrains.Annotations;

namespace JsonApiDotNetCore.Repositories;

/// <summary>
/// The error that is thrown when the underlying data store is unable to persist changes.
/// </summary>
[PublicAPI]
public sealed class DataStoreUpdateException(Exception? innerException)
    : Exception("Failed to persist changes in the underlying data store.", innerException);
