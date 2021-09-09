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
    }
}
