using System;
using System.Linq;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Repositories
{
    public static class DbContextExtensions
    {
        internal static object GetTrackedIdentifiable(this DbContext context, IIdentifiable identifiable)
        {
            if (identifiable == null) throw new ArgumentNullException(nameof(identifiable));

            var entityType = identifiable.GetType();
            var entityEntry = context.ChangeTracker
                .Entries()
                .FirstOrDefault(entry =>
                    entry.Entity.GetType() == entityType &&
                    ((IIdentifiable) entry.Entity).StringId == identifiable.StringId);

            return entityEntry?.Entity;
        }
    }
}
