using System.Reflection;
using JsonApiDotNetCore.OpenApi.Client;

namespace OpenApiClientTests;

internal static class ObjectExtensions
{
    public static object? GetPropertyValue(this object source, string propertyName)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(propertyName);

        PropertyInfo propertyInfo = source.GetType().GetProperties().Single(attribute => attribute.Name == propertyName);

        return propertyInfo.GetValue(source);
    }

    public static void SetPropertyValue(this object source, string propertyName, object? value)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(propertyName);

        PropertyInfo propertyInfo = source.GetType().GetProperties().Single(attribute => attribute.Name == propertyName);

        propertyInfo.SetValue(source, value);
    }

    public static object? GetDefaultValueForProperty(this object source, string propertyName)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(propertyName);

        PropertyInfo propertyInfo = source.GetType().GetProperties().Single(attribute => attribute.Name == propertyName);

        return Activator.CreateInstance(propertyInfo.PropertyType);
    }
}
