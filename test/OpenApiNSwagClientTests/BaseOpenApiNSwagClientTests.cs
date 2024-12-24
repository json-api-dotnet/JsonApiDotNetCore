using System.Linq.Expressions;
using System.Reflection;

namespace OpenApiNSwagClientTests;

public abstract class BaseOpenApiNSwagClientTests
{
    private const string AttributesObjectParameterName = "attributesObject";

    protected static Expression<Func<TAttributesObject, object?>> CreateAttributeSelectorFor<TAttributesObject>(string propertyName)
        where TAttributesObject : class
    {
        Type attributesObjectType = typeof(TAttributesObject);

        ParameterExpression parameter = Expression.Parameter(attributesObjectType, AttributesObjectParameterName);
        MemberExpression property = Expression.Property(parameter, propertyName);
        UnaryExpression castToObject = Expression.Convert(property, typeof(object));

        return Expression.Lambda<Func<TAttributesObject, object?>>(castToObject, parameter);
    }

    /// <summary>
    /// Sets the property on the specified source to its default value (null for string, 0 for int, false for bool, etc).
    /// </summary>
    protected static object? SetPropertyToDefaultValue<T>(T source, string propertyName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(propertyName);

        PropertyInfo property = GetExistingProperty(typeof(T), propertyName);

        object? defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
        property.SetValue(source, defaultValue);

        return defaultValue;
    }

    /// <summary>
    /// Sets the property on the specified source to its initial value, when the type was constructed. This takes the presence of a type initializer into
    /// account.
    /// </summary>
    protected static void SetPropertyToInitialValue<T>(T source, string propertyName)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(propertyName);

        var emptyRelationshipsObject = new T();
        object? defaultValue = emptyRelationshipsObject.GetPropertyValue(propertyName);

        source.SetPropertyValue(propertyName, defaultValue);
    }

    /// <summary>
    /// Sets the 'Data' property of the specified relationship to <c>null</c>.
    /// </summary>
    protected static void SetDataPropertyToNull<T>(T source, string relationshipPropertyName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(relationshipPropertyName);

        PropertyInfo relationshipProperty = GetExistingProperty(typeof(T), relationshipPropertyName);
        object? relationshipValue = relationshipProperty.GetValue(source);

        if (relationshipValue == null)
        {
            throw new InvalidOperationException($"Property '{typeof(T).Name}.{relationshipPropertyName}' is null.");
        }

        PropertyInfo dataProperty = GetExistingProperty(relationshipProperty.PropertyType, "Data");
        dataProperty.SetValue(relationshipValue, null);
    }

    private static PropertyInfo GetExistingProperty(Type type, string propertyName)
    {
        PropertyInfo? property = type.GetProperty(propertyName);

        if (property == null)
        {
            throw new InvalidOperationException($"Type '{type.Name}' does not contain a property named '{propertyName}'.");
        }

        return property;
    }
}
