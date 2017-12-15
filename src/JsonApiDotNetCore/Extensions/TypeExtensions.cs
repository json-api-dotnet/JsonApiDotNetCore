using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Extensions
{
    internal static class TypeExtensions
    {
        public static Type GetElementType(this IEnumerable enumerable)
        {
            var enumerableTypes = enumerable.GetType()
                .GetInterfaces()
                .Where(t => t.IsGenericType == true && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .ToList();

            var numberOfEnumerableTypes = enumerableTypes.Count;

            if (numberOfEnumerableTypes == 0)
            {
                throw new ArgumentException($"{nameof(enumerable)} of type {enumerable.GetType().FullName} does not implement a generic variant of {nameof(IEnumerable)}");
            }

            if (numberOfEnumerableTypes > 1)
            {
                throw new ArgumentException($"{nameof(enumerable)} of type {enumerable.GetType().FullName} implements more than one generic variant of {nameof(IEnumerable)}:\n" +
                    $"{string.Join("\n", enumerableTypes.Select(t => t.FullName))}");
            }

            var elementType = enumerableTypes[0].GenericTypeArguments[0];

            return elementType;
        }
    }
}
