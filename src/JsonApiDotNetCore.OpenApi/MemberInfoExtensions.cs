using System;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi
{
    internal static class MemberInfoExtensions
    {
        public static TypeCategory GetTypeCategory(this MemberInfo source)
        {
            ArgumentGuard.NotNull(source, nameof(source));

            Type memberType = source.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)source).FieldType,
                MemberTypes.Property => ((PropertyInfo)source).PropertyType,
                _ => throw new ArgumentException("Cannot get the type category for members of type other than 'MemberTypes.Field' or 'MemberTypes.Property'.")
            };

            if (memberType.IsValueType)
            {
                return Nullable.GetUnderlyingType(memberType) != null ? TypeCategory.NullableValueType : TypeCategory.ValueType;
            }

            // Once we switch to .NET 6, we should rely instead on the built-in reflection APIs for nullability information. See https://devblogs.microsoft.com/dotnet/announcing-net-6-preview-7/#libraries-reflection-apis-for-nullability-information.
            return source.IsNonNullableReferenceType() ? TypeCategory.NonNullableReferenceType : TypeCategory.NullableReferenceType;
        }
    }
}
