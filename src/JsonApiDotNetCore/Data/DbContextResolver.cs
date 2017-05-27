using System;
using JsonApiDotNetCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
    public class DbContextResolver : IDbContextResolver
    {
        private readonly DbContext _context;

        public DbContextResolver(DbContext context)
        {
            _context = context;
        }

        public DbContext GetContext() => _context;

        public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class
            => _context.GetDbSet<TEntity>();
    }
}
