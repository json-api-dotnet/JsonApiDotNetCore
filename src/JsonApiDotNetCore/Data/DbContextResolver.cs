using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
    public class DbContextResolver<TContext> : IDbContextResolver
        where TContext : DbContext
    {
        private readonly TContext _context;

        public DbContextResolver(TContext context)
        {
            _context = context;
        }

        public DbContext GetContext() => _context;

        public DbSet<TResource> GetDbSet<TResource>() where TResource : class =>  null;

    }
}
