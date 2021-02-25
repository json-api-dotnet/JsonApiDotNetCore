using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Provides a method to resolve a <see cref="DbContext" />.
    /// </summary>
    public interface IDbContextResolver
    {
        DbContext GetContext();
    }
}
