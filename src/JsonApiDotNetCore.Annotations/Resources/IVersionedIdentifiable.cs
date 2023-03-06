using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <summary>
/// Defines the basic contract for a JSON:API resource that uses optimistic concurrency. All resource classes must implement
/// <see cref="IVersionedIdentifiable{TId, TVersion}" />.
/// </summary>
public interface IVersionedIdentifiable : IIdentifiable
{
    /// <summary>
    /// The value for element 'version' in a JSON:API request or response.
    /// </summary>
    string? Version { get; set; }
}

/// <summary>
/// When implemented by a class, indicates to JsonApiDotNetCore that the class represents a JSON:API resource that uses optimistic concurrency.
/// </summary>
/// <typeparam name="TId">
/// The resource identifier type.
/// </typeparam>
/// <typeparam name="TVersion">
/// The database vendor-specific type that is used to store the concurrency token.
/// </typeparam>
[PublicAPI]
public interface IVersionedIdentifiable<TId, TVersion> : IIdentifiable<TId>, IVersionedIdentifiable
{
    /// <summary>
    /// The concurrency token, which is used to detect if the resource was modified by another user since the moment this resource was last retrieved.
    /// </summary>
    TVersion ConcurrencyToken { get; set; }

    /// <summary>
    /// Represents a database column where random data is written to on updates, in order to force a concurrency check during relationship updates.
    /// </summary>
    Guid ConcurrencyValue { get; set; }
}
