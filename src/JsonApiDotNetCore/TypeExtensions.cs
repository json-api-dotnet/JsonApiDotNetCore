using System;
using System.Linq;

namespace JsonApiDotNetCore
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Whether the specified source type implements or equals the specified interface.
        /// </summary>
        public static bool IsOrImplementsInterface(this Type source, Type interfaceType)
        {
            ArgumentGuard.NotNull(interfaceType, nameof(interfaceType));

            if (source == null)
            {
                return false;
            }

            return source == interfaceType || source.GetInterfaces().Any(type => type == interfaceType);
        }

        /// <summary>
        /// Gets the name of a type, including the names of its generic type parameters.
        /// <example>
        /// <code><![CDATA[
        /// KeyValuePair<TimeSpan, Nullable<DateTimeOffset>>
        /// ]]></code>
        /// </example>
        /// </summary>
        public static string GetFriendlyTypeName(this Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            // Based on https://stackoverflow.com/questions/2581642/how-do-i-get-the-type-name-of-a-generic-type-argument.

            if (type.IsGenericType)
            {
                string genericArguments = type.GetGenericArguments().Select(GetFriendlyTypeName)
                    .Aggregate((firstType, secondType) => $"{firstType}, {secondType}");

                return $"{type.Name[..type.Name.IndexOf("`", StringComparison.Ordinal)]}" + $"<{genericArguments}>";
            }

            return type.Name;
        }
    }
}
