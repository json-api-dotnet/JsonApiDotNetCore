using System.Reflection;
using JsonApiDotNetCore.OpenApi.Client;

namespace OpenApiClientTests;

internal static class ObjectExtensions
{
    public static void SetPropertyToDefaultValue(this object target, string propertyName)
    {
        ArgumentGuard.NotNull(target);
        ArgumentGuard.NotNull(propertyName);

        Type declaringType = target.GetType();

        PropertyInfo property = declaringType.GetProperties().Single(property => property.Name == propertyName);
        object? defaultValue = declaringType.IsValueType ? Activator.CreateInstance(declaringType) : null;

        property.SetValue(target, defaultValue);
    }
}
