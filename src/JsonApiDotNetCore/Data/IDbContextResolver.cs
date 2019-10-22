using System;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
    public interface IDbContextResolver
    {
        DbContext GetContext();

        [Obsolete("Use DbContext.Set<TResource>() instead", error: true)]
        DbSet<TResource> GetDbSet<TResource>() 
            where TResource : class;
    }
}
