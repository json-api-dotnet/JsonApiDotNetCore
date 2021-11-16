using System.Reflection;
using JsonApiDotNetCore.Resources.Internal;

namespace JsonApiDotNetCore.Resources;

internal static class IdentifiableExtensions
{
    private const string IdPropertyName = nameof(Identifiable<object>.Id);

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
}
