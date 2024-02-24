using System.Reflection;
using JsonApiDotNetCore.OpenApi.Client.NSwag;

namespace OpenApiClientTests;

internal static class ObjectExtensions
{
    public static object? GetPropertyValue(this object source, string propertyName)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(propertyName);

        PropertyInfo propertyInfo = GetExistingProperty(source.GetType(), propertyName);
        return propertyInfo.GetValue(source);
    }

    public static void SetPropertyValue(this object source, string propertyName, object? value)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(propertyName);

        PropertyInfo propertyInfo = GetExistingProperty(source.GetType(), propertyName);
        propertyInfo.SetValue(source, value);
    }

    private static PropertyInfo GetExistingProperty(Type type, string propertyName)
    {
        PropertyInfo? propertyInfo = type.GetProperty(propertyName);

        if (propertyInfo == null)
        {
            throw new InvalidOperationException($"Type '{type}' does not contain a property named '{propertyName}'.");
        }

        return propertyInfo;
    }
}
