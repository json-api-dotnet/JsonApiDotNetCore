using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MultiDbContextExample.Data
{
    public sealed class DbContextAResolver : IDbContextResolver
    {
        private readonly DbContextA _dbContextA;

        public DbContextAResolver(DbContextA dbContextA)
        {
            _dbContextA = dbContextA;
        }

        public DbContext GetContext()
        {
            return _dbContextA;
        }
    }
}
