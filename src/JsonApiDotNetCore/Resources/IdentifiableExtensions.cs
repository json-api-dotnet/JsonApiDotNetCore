using System;
using System.Reflection;

namespace JsonApiDotNetCore.Resources
{
    internal static class IdentifiableExtensions
    {
        public static object GetTypedId(this IIdentifiable identifiable)
        {
            ArgumentGuard.NotNull(identifiable, nameof(identifiable));

            PropertyInfo property = identifiable.GetType().GetProperty(nameof(Identifiable.Id));

            if (property == null)
            {
                throw new InvalidOperationException($"Resource of type '{identifiable.GetType()}' does not have an 'Id' property.");
            }

            return property.GetValue(identifiable);
        }
    }
}
