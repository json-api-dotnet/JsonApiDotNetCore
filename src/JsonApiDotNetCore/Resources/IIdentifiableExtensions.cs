using System;
using System.Reflection;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Resources
{
    public static class IIdentifiableExtensions
    {
        internal static object GetTypedId(this IIdentifiable identifiable)
        {
            if (identifiable == null) throw new ArgumentNullException(nameof(identifiable));
            
            PropertyInfo property = identifiable.GetType().GetProperty(nameof(Identifiable.Id));
            
            return property.GetValue(identifiable);
        }
    }
}
