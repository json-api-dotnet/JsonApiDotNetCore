using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

#pragma warning disable AV1008 // Class should not be static

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
        public static void NotNullNorEmpty<T>([CanBeNull] IEnumerable<T> value, [NotNull] [InvokerParameterName] string name,
            [CanBeNull] string collectionName = null)
        {
            NotNull(value, name);

            if (!value.Any())
            {
                throw new ArgumentException($"Must have one or more {collectionName ?? name}.", name);
            }
        }

        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNullNorEmpty([CanBeNull] string value, [NotNull] [InvokerParameterName] string name)
        {
            NotNull(value, name);

            if (value == string.Empty)
            {
                throw new ArgumentException("String cannot be null or empty.", name);
            }
        }
    }
}
