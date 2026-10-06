using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Repositories;

/// <inheritdoc cref="IReferencingForeignKeysCache" />
[PublicAPI]
public sealed class ReferencingForeignKeysCache : IReferencingForeignKeysCache
{
    private static readonly ReadOnlyCollection<IReadOnlyForeignKey> EmptyCollection = Array.Empty<IReadOnlyForeignKey>().AsReadOnly();
    private readonly ConcurrentDictionary<CacheKey, IReadOnlyList<IReadOnlyForeignKey>> _foreignKeysCache = new();

    /// <inheritdoc />
    public IReadOnlyList<IReadOnlyForeignKey> GetForeignKeysRequiringClientSetNullOnDelete(IReadOnlyModel model, Type entityClrType)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entityClrType);

        var cacheKey = new CacheKey(model, entityClrType);
        return _foreignKeysCache.GetOrAdd(cacheKey, static key => ResolveForeignKeys(key.Model, key.EntityClrType));
    }

    private static ReadOnlyCollection<IReadOnlyForeignKey> ResolveForeignKeys(IReadOnlyModel model, Type entityClrType)
    {
        IReadOnlyEntityType? entityType = model.FindEntityType(entityClrType);

        if (entityType != null)
        {
            IReadOnlyForeignKey[] foreignKeys = entityType.GetReferencingForeignKeys()
                .Where(foreignKey => foreignKey.DeleteBehavior == DeleteBehavior.ClientSetNull).ToArray();

            if (foreignKeys.Length > 0)
            {
                return foreignKeys.AsReadOnly();
            }
        }

        return EmptyCollection;
    }

    private record CacheKey(IReadOnlyModel Model, Type EntityClrType);
}
