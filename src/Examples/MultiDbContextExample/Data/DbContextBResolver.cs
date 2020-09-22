using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MultiDbContextExample.Data
{
    public sealed class DbContextBResolver : IDbContextResolver
    {
        private readonly DbContextB _dbContextB;

        public DbContextBResolver(DbContextB dbContextB)
        {
            _dbContextB = dbContextB;
        }

        public DbContext GetContext()
        {
            return _dbContextB;
        }
    }
}
