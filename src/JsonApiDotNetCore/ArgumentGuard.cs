using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace JsonApiDotNetCore
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

        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNullNorEmpty<T>([CanBeNull] IEnumerable<T> value, [NotNull] [InvokerParameterName] string name)
        {
            NotNull(value, name);

            if (!value.Any())
            {
                throw new ArgumentException("Collection cannot be empty.", name);
            }
        }
    }
}
