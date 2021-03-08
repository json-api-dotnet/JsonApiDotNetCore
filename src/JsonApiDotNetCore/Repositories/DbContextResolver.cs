using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Repositories
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class DbContextResolver<TDbContext> : IDbContextResolver
        where TDbContext : DbContext
    {
        private readonly TDbContext _context;

        public DbContextResolver(TDbContext context)
        {
            ArgumentGuard.NotNull(context, nameof(context));

            _context = context;
        }

        public DbContext GetContext()
        {
            return _context;
        }

        public TDbContext GetTypedContext()
        {
            return _context;
        }
    }
}
