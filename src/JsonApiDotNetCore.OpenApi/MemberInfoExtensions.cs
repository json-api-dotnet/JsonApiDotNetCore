using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi;

internal static class MemberInfoExtensions
{
    public static TypeCategory GetTypeCategory(this MemberInfo source)
    {
        ArgumentGuard.NotNull(source, nameof(source));

        Type memberType;

        if (source.MemberType.HasFlag(MemberTypes.Field))
        {
            memberType = ((FieldInfo)source).FieldType;
        }
        else if (source.MemberType.HasFlag(MemberTypes.Property))
        {
            memberType = ((PropertyInfo)source).PropertyType;
        }
        else
        {
            throw new NotSupportedException($"Member type '{source.MemberType}' must be a property or field.");
        }

        if (memberType.IsValueType)
        {
            return Nullable.GetUnderlyingType(memberType) != null ? TypeCategory.NullableValueType : TypeCategory.ValueType;
        }

        // Once we switch to .NET 6, we should rely instead on the built-in reflection APIs for nullability information.
        // See https://devblogs.microsoft.com/dotnet/announcing-net-6-preview-7/#libraries-reflection-apis-for-nullability-information.
        return source.IsNonNullableReferenceType() ? TypeCategory.NonNullableReferenceType : TypeCategory.NullableReferenceType;
    }
}
