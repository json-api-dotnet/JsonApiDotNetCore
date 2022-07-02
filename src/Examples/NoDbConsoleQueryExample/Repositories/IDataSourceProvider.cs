using JsonApiDotNetCore.Resources;

namespace NoDbConsoleQueryExample.Repositories;

/// <summary>
/// Provides access to in-memory data (when not using a database).
/// </summary>
public interface IDataSourceProvider<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    void Set(IEnumerable<TResource> resources);

    IEnumerable<TResource> Get();
}
