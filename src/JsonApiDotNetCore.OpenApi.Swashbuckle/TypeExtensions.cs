namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class TypeExtensions
{
    public static Type ConstructedToOpenType(this Type type)
    {
        ArgumentGuard.NotNull(type);

        return type.IsConstructedGenericType ? type.GetGenericTypeDefinition() : type;
    }
}
