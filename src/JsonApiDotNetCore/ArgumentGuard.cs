using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore
{
    internal static class ArgumentGuard
    {
        [AssertionMethod]
        public static void NotNull<T>([NoEnumeration] [SysNotNull] T? value, [InvokerParameterName] string name)
            where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }
        }

        [AssertionMethod]
        public static void NotNullNorEmpty<T>([SysNotNull] IEnumerable<T>? value, [InvokerParameterName] string name, string? collectionName = null)
        {
            NotNull(value, name);

            if (!value.Any())
            {
                throw new ArgumentException($"Must have one or more {collectionName ?? name}.", name);
            }
        }

        [AssertionMethod]
        public static void NotNullNorEmpty([SysNotNull] string? value, [InvokerParameterName] string name)
        {
            NotNull(value, name);

            if (value == string.Empty)
            {
                throw new ArgumentException("String cannot be null or empty.", name);
            }
        }
    }
}
