using System.Reflection;

namespace JsonApiDotNetCore.Resources;

internal static class IdentifiableExtensions
{
    private const string IdPropertyName = nameof(Identifiable<>.Id);

    public static object GetTypedId(this IIdentifiable identifiable)
    {
        ArgumentNullException.ThrowIfNull(identifiable);

        PropertyInfo? property = identifiable.GetClrType().GetProperty(IdPropertyName);

        if (property == null)
        {
            throw new InvalidOperationException($"Resource of type '{identifiable.GetClrType()}' does not contain a property named '{IdPropertyName}'.");
        }

        object? propertyValue = property.GetValue(identifiable);
        object? defaultValue = RuntimeTypeConverter.GetDefaultValue(property.PropertyType);

        if (Equals(propertyValue, defaultValue))
        {
            throw new InvalidOperationException($"Property '{identifiable.GetClrType().Name}.{IdPropertyName}' should " +
                $"have been assigned at this point, but it contains its default {property.PropertyType.Name} value '{propertyValue}'.");
        }

        return propertyValue!;
    }

    public static Type GetClrType(this IIdentifiable identifiable)
    {
        ArgumentNullException.ThrowIfNull(identifiable);

        return identifiable is IAbstractResourceWrapper abstractResource ? abstractResource.AbstractType : identifiable.GetType();
    }
}
