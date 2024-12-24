using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JsonApiDotNetCore.Repositories;

[PublicAPI]
public static class DbContextExtensions
{
    /// <summary>
    /// If not already tracked, attaches the specified resource to the change tracker in <see cref="EntityState.Unchanged" /> state.
    /// </summary>
    public static IIdentifiable GetTrackedOrAttach(this DbContext dbContext, IIdentifiable resource)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(resource);

        var trackedIdentifiable = (IIdentifiable?)dbContext.GetTrackedIdentifiable(resource);

        if (trackedIdentifiable == null)
        {
            dbContext.Entry(resource).State = EntityState.Unchanged;
            trackedIdentifiable = resource;
        }

        return trackedIdentifiable;
    }

    /// <summary>
    /// Searches the change tracker for an entity that matches the type and ID of <paramref name="identifiable" />.
    /// </summary>
    public static object? GetTrackedIdentifiable(this DbContext dbContext, IIdentifiable identifiable)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(identifiable);

        Type resourceClrType = identifiable.GetClrType();
        string? stringId = identifiable.StringId;

        EntityEntry? entityEntry = dbContext.ChangeTracker.Entries().FirstOrDefault(entry => IsResource(entry, resourceClrType, stringId));

        return entityEntry?.Entity;
    }

    private static bool IsResource(EntityEntry entry, Type resourceClrType, string? stringId)
    {
        return entry.Entity.GetType() == resourceClrType && ((IIdentifiable)entry.Entity).StringId == stringId;
    }

    /// <summary>
    /// Detaches all entities from the change tracker.
    /// </summary>
    public static void ResetChangeTracker(this DbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        dbContext.ChangeTracker.Clear();
    }
}
