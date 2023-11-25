namespace JsonApiDotNetCore;

internal static class TypeExtensions
{
    /// <summary>
    /// Whether the specified source type implements or equals the specified interface.
    /// </summary>
    public static bool IsOrImplementsInterface<TInterface>(this Type? source)
    {
        return IsOrImplementsInterface(source, typeof(TInterface));
    }

    /// <summary>
    /// Whether the specified source type implements or equals the specified interface. This overload enables to test for an open generic interface.
    /// </summary>
    private static bool IsOrImplementsInterface(this Type? source, Type interfaceType)
    {
        ArgumentGuard.NotNull(interfaceType);

        if (source == null)
        {
            return false;
        }

        return AreTypesEqual(interfaceType, source, interfaceType.IsGenericType) ||
            source.GetInterfaces().Any(type => AreTypesEqual(interfaceType, type, interfaceType.IsGenericType));
    }

    private static bool AreTypesEqual(Type left, Type right, bool isLeftGeneric)
    {
        return isLeftGeneric ? right.IsGenericType && right.GetGenericTypeDefinition() == left : left == right;
    }

    /// <summary>
    /// Gets the name of a type, including the names of its generic type arguments.
    /// <example>
    /// <code><![CDATA[
    /// KeyValuePair<TimeSpan, Nullable<DateTimeOffset>>
    /// ]]></code>
    /// </example>
    /// </summary>
    public static string GetFriendlyTypeName(this Type type)
    {
        ArgumentGuard.NotNull(type);

        // Based on https://stackoverflow.com/questions/2581642/how-do-i-get-the-type-name-of-a-generic-type-argument.

        if (type.IsGenericType)
        {
            string typeArguments = type.GetGenericArguments().Select(GetFriendlyTypeName).Aggregate((firstType, secondType) => $"{firstType}, {secondType}");
            return $"{type.Name[..type.Name.IndexOf("`", StringComparison.Ordinal)]}<{typeArguments}>";
        }

        return type.Name;
    }
}
