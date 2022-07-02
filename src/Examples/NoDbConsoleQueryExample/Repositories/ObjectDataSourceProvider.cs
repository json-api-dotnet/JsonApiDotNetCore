using JsonApiDotNetCore.Resources;

namespace NoDbConsoleQueryExample.Repositories;

/// <inheritdoc />
internal sealed class ObjectDataSourceProvider<TResource, TId> : IDataSourceProvider<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private IEnumerable<TResource> _resources = Enumerable.Empty<TResource>();

    public void Set(IEnumerable<TResource> resources)
    {
        _resources = resources;
    }

    public IEnumerable<TResource> Get()
    {
        return _resources;
    }
}
