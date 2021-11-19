using System;
using System.Reflection;
using JsonApiDotNetCore.Resources.Internal;

namespace JsonApiDotNetCore.Resources
{
    internal static class IdentifiableExtensions
    {
        private const string IdPropertyName = nameof(Identifiable<object>.Id);

        public static object GetTypedId(this IIdentifiable identifiable)
        {
            ArgumentGuard.NotNull(identifiable, nameof(identifiable));

            PropertyInfo? property = identifiable.GetType().GetProperty(IdPropertyName);

            if (property == null)
            {
                throw new InvalidOperationException($"Resource of type '{identifiable.GetType()}' does not contain a property named '{IdPropertyName}'.");
            }

            object? propertyValue = property.GetValue(identifiable);

            // PERF: We want to throw when 'Id' is unassigned without doing an expensive reflection call, unless this is likely the case.
            if (identifiable.StringId == null)
            {
                object? defaultValue = RuntimeTypeConverter.GetDefaultValue(property.PropertyType);

                if (Equals(propertyValue, defaultValue))
                {
                    throw new InvalidOperationException($"Property '{identifiable.GetType().Name}.{IdPropertyName}' should " +
                        $"have been assigned at this point, but it contains its default {property.PropertyType.Name} value '{propertyValue}'.");
                }
            }

            return propertyValue!;
        }
    }
}
