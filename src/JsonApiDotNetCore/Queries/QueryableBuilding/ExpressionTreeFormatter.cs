using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <summary>
/// Converts a <see cref="Expression" /> to readable text, if the AgileObjects.ReadableExpressions NuGet package is referenced.
/// </summary>
internal sealed class ExpressionTreeFormatter
{
    private static readonly Lazy<MethodInvoker?> LazyToReadableStringMethod = new(GetToReadableStringMethod, LazyThreadSafetyMode.ExecutionAndPublication);

    public static ExpressionTreeFormatter Instance { get; } = new();

    private ExpressionTreeFormatter()
    {
    }

    private static MethodInvoker? GetToReadableStringMethod()
    {
        Assembly? assembly = TryLoadAssembly();
        Type? type = assembly?.GetType("AgileObjects.ReadableExpressions.ExpressionExtensions", false);
        MethodInfo? method = type?.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(method => method.Name == "ToReadableString");
        return method != null ? MethodInvoker.Create(method) : null;
    }

    private static Assembly? TryLoadAssembly()
    {
        try
        {
            return Assembly.Load("AgileObjects.ReadableExpressions");
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or BadImageFormatException)
        {
        }

        return null;
    }

    public string? GetText(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        try
        {
            return LazyToReadableStringMethod.Value?.Invoke(null, expression, null) as string;
        }
        catch (Exception exception) when (exception is TargetException or InvalidOperationException or TargetParameterCountException or NotSupportedException)
        {
            return null;
        }
    }
}
