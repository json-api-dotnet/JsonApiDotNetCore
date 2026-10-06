using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Repositories;

/// <summary>
/// Provides a method to resolve a <see cref="DbContext" />.
/// </summary>
public interface IDbContextResolver
{
    DbContext GetContext();

    /// <summary>
    /// Returns the referencing foreign keys that have <see cref="DeleteBehavior.ClientSetNull" /> for the specified principal entity type.
    /// </summary>
    IReadOnlyList<IReadOnlyForeignKey> GetForeignKeysRequiringClientSetNullOnDelete(Type entityClrType);
}
