using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JsonApiDotNetCore.Repositories
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// If not already tracked, attaches the specified resource to the change tracker in <see cref="EntityState.Unchanged"/> state.
        /// </summary>
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

        /// <summary>
        /// Searches the change tracker for an entity that matches the type and ID of <paramref name="identifiable"/>.
        /// </summary>
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

        /// <summary>
        /// Detaches all entities from the change tracker.
        /// </summary>
        public static void ResetChangeTracker(this DbContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            List<EntityEntry> entriesWithChanges = dbContext.ChangeTracker.Entries().ToList();

            foreach (EntityEntry entry in entriesWithChanges)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}
