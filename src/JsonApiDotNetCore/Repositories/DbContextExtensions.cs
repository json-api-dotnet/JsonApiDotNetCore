using System;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

        public static IProperty GetSingleForeignKeyProperty(this DbContext dbContext, HasOneAttribute relationship)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            
            var entityType = dbContext.Model.FindEntityType(relationship.LeftType);
            var foreignKeyProperties = entityType.FindNavigation(relationship.Property.Name).ForeignKey.Properties;

            if (foreignKeyProperties.Count != 1)
            {
                throw new ArgumentException($"Relationship {relationship} does not have a left side with a single foreign key");
            }
            
            return foreignKeyProperties.First();
        }
        
        private static object GetTrackedIdentifiable(this DbContext dbContext, IIdentifiable identifiable)
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
    }
}
