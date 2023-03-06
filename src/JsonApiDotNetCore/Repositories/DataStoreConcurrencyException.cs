using JetBrains.Annotations;

namespace JsonApiDotNetCore.Repositories;

/// <summary>
/// The error that is thrown when the resource version from the request does not match the server version.
/// </summary>
[PublicAPI]
public sealed class DataStoreConcurrencyException : DataStoreUpdateException
{
    public DataStoreConcurrencyException(Exception? innerException)
        : base("The resource version does not match the server version. This indicates that data has been modified since the resource was retrieved.",
            innerException)
    {
    }
}
