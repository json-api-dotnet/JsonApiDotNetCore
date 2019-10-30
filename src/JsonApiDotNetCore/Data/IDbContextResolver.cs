using System;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
    public interface IDbContextResolver
    {
        DbContext GetContext();

        [Obsolete("Use DbContext.Set<TEntity>() instead", error: true)]
        DbSet<TEntity> GetDbSet<TEntity>() 
            where TEntity : class;
    }
}
