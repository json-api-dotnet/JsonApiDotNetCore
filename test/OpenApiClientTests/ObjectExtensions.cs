using System.Collections.Concurrent;
using System.Reflection;
using JsonApiDotNetCore.OpenApi.Client;

namespace OpenApiClientTests;

internal static class ObjectExtensions
{
    private static readonly ConcurrentDictionary<string, PropertyInfo> PropertyInfoCache = new();

    public static object? GetPropertyValue(this object source, string propertyName)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(propertyName);

        string cacheKey = EnsurePropertyInfoIsCached(source, propertyName);

        return PropertyInfoCache[cacheKey].GetValue(source);
    }

    private static string EnsurePropertyInfoIsCached(object source, string propertyName)
    {
        string cacheKey = $"{source.GetType().FullName}-{propertyName}";

        if (!PropertyInfoCache.ContainsKey(cacheKey))
        {
            PropertyInfo propertyInfo = source.GetType().GetProperties().Single(attribute => attribute.Name == propertyName);
            PropertyInfoCache[cacheKey] = propertyInfo;
        }

        return cacheKey;
    }

    public static void SetPropertyValue(this object source, string propertyName, object? value)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(propertyName);

        string cacheKey = EnsurePropertyInfoIsCached(source, propertyName);

        PropertyInfoCache[cacheKey].SetValue(source, value);
    }

    public static object? GetDefaultValueForProperty(this object source, string propertyName)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(propertyName);

        string cacheKey = EnsurePropertyInfoIsCached(source, propertyName);

        return Activator.CreateInstance(PropertyInfoCache[cacheKey].PropertyType);
    }
}
