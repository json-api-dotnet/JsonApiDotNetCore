namespace JsonApiDotNetCore;

internal static class TypeExtensions
{
    /// <summary>
    /// Whether the specified source type implements or equals the specified interface.
    /// </summary>
    public static bool IsOrImplementsInterface<TInterface>(this Type? source)
    {
        return source.IsOrImplementsInterface(typeof(TInterface));
    }

    /// <summary>
    /// Whether the specified source type implements or equals the specified interface. This overload enables testing for an open generic interface.
    /// </summary>
    private static bool IsOrImplementsInterface(this Type? source, Type interfaceType)
    {
        ArgumentNullException.ThrowIfNull(interfaceType);

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
}
