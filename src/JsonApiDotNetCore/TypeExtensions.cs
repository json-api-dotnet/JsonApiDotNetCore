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
        /// Whether the specified source type inherits from the specified open generic type.
        /// </summary>
        internal static bool IsSubclassOfOpenGeneric(this Type source, Type openGenericType)
        {
            ArgumentGuard.NotNull(openGenericType, nameof(openGenericType));
            ArgumentGuard.NotNull(source, nameof(openGenericType));

            // TODO: check if source should be allowed null and return false in that case?

            Type typeInInheritanceTreeOfSource = source;

            while (typeInInheritanceTreeOfSource != null && typeInInheritanceTreeOfSource != typeof(object))
            {
                Type typeToCheck = typeInInheritanceTreeOfSource.IsGenericType
                    ? typeInInheritanceTreeOfSource.GetGenericTypeDefinition()
                    : typeInInheritanceTreeOfSource;

                if (openGenericType == typeToCheck)
                {
                    return true;
                }

                typeInInheritanceTreeOfSource = typeInInheritanceTreeOfSource.BaseType;
            }

            return false;
        }
    }
}
