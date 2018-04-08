using System;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Extensions
{
    public static class DbContextExtensions
    {
        [Obsolete("This is no longer required since the introduction of context.Set<T>", error: false)]
        public static DbSet<T> GetDbSet<T>(this DbContext context) where T : class 
            => context.Set<T>();
    }
}
