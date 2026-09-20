using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Repositories;

/// <summary>
/// Caches referencing Entity Framework Core foreign keys that have <see cref="DeleteBehavior.ClientSetNull" /> for a principal entity type.
/// </summary>
[PublicAPI]
public interface IReferencingForeignKeysCache
{
    /// <summary>
    /// Returns the referencing foreign keys that have <see cref="DeleteBehavior.ClientSetNull" /> for the specified principal entity type.
    /// </summary>
    IReadOnlyList<IReadOnlyForeignKey> GetForeignKeysRequiringClientSetNullOnDelete(IReadOnlyModel model, Type entityClrType);
}
