using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JsonApiDotNetCore.Repositories;

[PublicAPI]
public static class DbContextExtensions
{
    private static readonly MethodInfo DbContextSetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!;

    // @formatter:wrap_chained_method_calls chop_if_long
    // @formatter:wrap_before_first_method_call true

    private static readonly MethodInfo QueryableLoadAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(method => method.Name == nameof(EntityFrameworkQueryableExtensions.LoadAsync))
        .Single(method => method.GetParameters().Length == 2);

    // @formatter:wrap_before_first_method_call restore
    // @formatter:wrap_chained_method_calls restore

    // @formatter:wrap_chained_method_calls chop_if_long
    // @formatter:wrap_before_first_method_call true

    private static readonly MethodInfo QueryableWhereMethod = typeof(Queryable)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(method => method.Name == nameof(Queryable.Where))
        .Single(method => method.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2);

    // @formatter:wrap_before_first_method_call restore
    // @formatter:wrap_chained_method_calls restore

    /// <summary>
    /// If not already tracked, attaches the specified resource to the change tracker in <see cref="EntityState.Unchanged" /> state.
    /// </summary>
    /// <remarks>
    /// Repeated calls to this method in repository operations may appear redundant at first glance, but are intentional safeguards. User-defined resource
    /// definition callbacks execute external code that may query or attach entities into the change tracker. Calling this method ensures the repository
    /// always transitions from detached placeholders to the actively tracked instances managed by EF Core.
    /// </remarks>
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

    internal static IQueryable Set(this DbContext context, Type entityType)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entityType);

        MethodInfo setMethod = DbContextSetMethod.MakeGenericMethod(entityType);
        return (IQueryable)setMethod.Invoke(context, null)!;
    }

    internal static Task LoadAsync(this IQueryable source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);

        MethodInfo loadAsyncMethod = QueryableLoadAsyncMethod.MakeGenericMethod(source.ElementType);

        return (Task)loadAsyncMethod.Invoke(null, [
            source,
            cancellationToken
        ])!;
    }

    internal static IQueryable Where(this IQueryable source, LambdaExpression predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        MethodInfo whereMethod = QueryableWhereMethod.MakeGenericMethod(source.ElementType);

        return (IQueryable)whereMethod.Invoke(null, [
            source,
            predicate
        ])!;
    }
}
