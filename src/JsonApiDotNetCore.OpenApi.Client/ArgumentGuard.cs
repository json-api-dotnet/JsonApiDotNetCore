using System;
using JetBrains.Annotations;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.OpenApi.Client
{
    internal static class ArgumentGuard
    {
        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNull<T>([CanBeNull] [NoEnumeration] T value, [NotNull] [InvokerParameterName] string name)
            where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}