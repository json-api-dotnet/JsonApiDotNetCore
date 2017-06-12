using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
    public interface IDbContextResolver
    {
        DbContext GetContext();
        DbSet<TEntity> GetDbSet<TEntity>() 
            where TEntity : class;
    }
}
