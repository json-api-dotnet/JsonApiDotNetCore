using System;
using System.Linq;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Extensions
{
    public static class DbContextExtensions
    {
        [Obsolete("This is no longer required since the introduction of context.Set<T>", error: false)]
        public static DbSet<T> GetDbSet<T>(this DbContext context) where T : class 
            => context.Set<T>();

        /// <summary>
        /// Determines whether or not EF is already tracking an entity of the same Type and Id
        /// </summary>
        public static bool EntityIsTracked(this DbContext context, IIdentifiable entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            var trackedEntries = context.ChangeTracker
                .Entries()
                .FirstOrDefault(entry => 
                    entry.Entity.GetType() == entity.GetType() 
                    && ((IIdentifiable)entry.Entity).StringId == entity.StringId
                );

            return trackedEntries != null;
        }
    }
}
