using System.Reflection;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class ObjectExtensions
{
    private static readonly Lazy<MethodInfo> MemberwiseCloneMethod =
        new(() => typeof(object).GetMethod(nameof(MemberwiseClone), BindingFlags.Instance | BindingFlags.NonPublic)!,
            LazyThreadSafetyMode.ExecutionAndPublication);

    public static T MemberwiseClone<T>(this T source)
        where T : class
    {
        ArgumentGuard.NotNull(source);

        return (T)MemberwiseCloneMethod.Value.Invoke(source, null)!;
    }
}
