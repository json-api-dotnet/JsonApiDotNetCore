using System.Reflection;
using JsonApiDotNetCore.Resources.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JsonApiDotNetCore.Resources;

internal static class IdentifiableExtensions
{
    private const string IdPropertyName = nameof(Identifiable<object>.Id);
    private const string ConcurrencyTokenPropertyName = nameof(IVersionedIdentifiable<object, object>.ConcurrencyToken);
    private const string ConcurrencyValuePropertyName = nameof(IVersionedIdentifiable<object, object>.ConcurrencyValue);

    public static object GetTypedId(this IIdentifiable identifiable)
    {
        ArgumentGuard.NotNull(identifiable);

        PropertyInfo? property = identifiable.GetClrType().GetProperty(IdPropertyName);

        if (property == null)
        {
            throw new InvalidOperationException($"Resource of type '{identifiable.GetClrType()}' does not contain a property named '{IdPropertyName}'.");
        }

        object? propertyValue = property.GetValue(identifiable);

        // PERF: We want to throw when 'Id' is unassigned without doing an expensive reflection call, unless this is likely the case.
        if (identifiable.StringId == null)
        {
            object? defaultValue = RuntimeTypeConverter.GetDefaultValue(property.PropertyType);

            if (Equals(propertyValue, defaultValue))
            {
                throw new InvalidOperationException($"Property '{identifiable.GetClrType().Name}.{IdPropertyName}' should " +
                    $"have been assigned at this point, but it contains its default {property.PropertyType.Name} value '{propertyValue}'.");
            }
        }

        return propertyValue!;
    }

    public static Type GetClrType(this IIdentifiable identifiable)
    {
        ArgumentGuard.NotNull(identifiable);

        return identifiable is IAbstractResourceWrapper abstractResource ? abstractResource.AbstractType : identifiable.GetType();
    }

    public static string? GetVersion(this IIdentifiable identifiable)
    {
        ArgumentGuard.NotNull(identifiable, nameof(identifiable));

        return identifiable is IVersionedIdentifiable versionedIdentifiable ? versionedIdentifiable.Version : null;
    }

    public static void SetVersion(this IIdentifiable identifiable, string? version)
    {
        ArgumentGuard.NotNull(identifiable, nameof(identifiable));

        if (identifiable is IVersionedIdentifiable versionedIdentifiable)
        {
            versionedIdentifiable.Version = version;
        }
    }

    public static void RestoreConcurrencyToken(this IIdentifiable identifiable, DbContext dbContext, string? versionFromRequest)
    {
        ArgumentGuard.NotNull(identifiable, nameof(identifiable));
        ArgumentGuard.NotNull(dbContext, nameof(dbContext));

        if (identifiable is IVersionedIdentifiable versionedIdentifiable)
        {
            versionedIdentifiable.Version = versionFromRequest;

            PropertyInfo? property = identifiable.GetClrType().GetProperty(ConcurrencyTokenPropertyName);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Resource of type '{identifiable.GetClrType()}' does not contain a property named '{ConcurrencyTokenPropertyName}'.");
            }

            PropertyEntry propertyEntry = dbContext.Entry(identifiable).Property(ConcurrencyTokenPropertyName);

            if (!propertyEntry.Metadata.IsConcurrencyToken)
            {
                throw new InvalidOperationException($"Property '{identifiable.GetClrType()}.{ConcurrencyTokenPropertyName}' is not a concurrency token.");
            }

            object? concurrencyTokenFromRequest = property.GetValue(identifiable);
            propertyEntry.OriginalValue = concurrencyTokenFromRequest;
        }
    }

    public static void RefreshConcurrencyValue(this IIdentifiable identifiable)
    {
        ArgumentGuard.NotNull(identifiable, nameof(identifiable));

        if (identifiable is IVersionedIdentifiable)
        {
            PropertyInfo? property = identifiable.GetClrType().GetProperty(ConcurrencyValuePropertyName);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Resource of type '{identifiable.GetClrType()}' does not contain a property named '{ConcurrencyValuePropertyName}'.");
            }

            property.SetValue(identifiable, Guid.NewGuid());
        }
    }
}
