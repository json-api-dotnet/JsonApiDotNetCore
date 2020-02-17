using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
    public sealed class DbContextResolver<TContext> : IDbContextResolver
        where TContext : DbContext
    {
        private readonly TContext _context;

        public DbContextResolver(TContext context)
        {
            _context = context;
        }

        public DbContext GetContext() => _context;
    }
}
