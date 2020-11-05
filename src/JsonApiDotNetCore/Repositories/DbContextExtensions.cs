using System;
using System.Linq;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Repositories
{
    public static class DbContextExtensions
    {
        public static IIdentifiable GetTrackedOrAttach(this DbContext dbContext, IIdentifiable resource)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var trackedIdentifiable = (IIdentifiable)dbContext.GetTrackedIdentifiable(resource);
            if (trackedIdentifiable == null)
            {
                dbContext.Entry(resource).State = EntityState.Unchanged;
                trackedIdentifiable = resource;
            }

            return trackedIdentifiable;
        }

        public static object GetTrackedIdentifiable(this DbContext dbContext, IIdentifiable identifiable)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
            if (identifiable == null) throw new ArgumentNullException(nameof(identifiable));

            var entityType = identifiable.GetType();
            var entityEntry = dbContext.ChangeTracker
                .Entries()
                .FirstOrDefault(entry =>
                    entry.Entity.GetType() == entityType &&
                    ((IIdentifiable) entry.Entity).StringId == identifiable.StringId);

            return entityEntry?.Entity;
        }
        
        public static IQueryable Set(this DbContext dbContext, Type entityType)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));

            var getDbSetOpen = typeof(DbContext).GetMethod(nameof(DbContext.Set));

            var getDbSetGeneric = getDbSetOpen.MakeGenericMethod(entityType);
            var dbSet = (IQueryable)getDbSetGeneric.Invoke(dbContext, null);

            return dbSet;
        }
    }
}
