using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.OpenApi
{
    internal static class MemberInfoExtensions
    {
        private const string NullableAttributeFullTypeName = "System.Runtime.CompilerServices.NullableAttribute";
        private const string NullableFlagsFieldName = "NullableFlags";
        private const string NullableContextAttributeFullTypeName = "System.Runtime.CompilerServices.NullableContextAttribute";
        private const string FlagFieldName = "Flag";

        /// <summary>
        /// Resolves the class of data type of the
        /// <param name="memberInfo"></param>
        /// .
        /// </summary>
        public static DataTypeClass ResolveDataType(this MemberInfo memberInfo)
        {
            ArgumentGuard.NotNull(memberInfo, nameof(memberInfo));

            // Based on https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/67344fe0b7c7e78128159d8bf02ebfe91408c3da/src/Swashbuckle.AspNetCore.SwaggerGen/SchemaGenerator/MemberInfoExtensions.cs#L36

            Type memberType = memberInfo.MemberType == MemberTypes.Field ? ((FieldInfo)memberInfo).FieldType : ((PropertyInfo)memberInfo).PropertyType;

            Type? underlyingType = Nullable.GetUnderlyingType(memberType);

            if (underlyingType != null)
            {
                return underlyingType.IsValueType ? DataTypeClass.NullableValueType : DataTypeClass.NullableReferenceType;
            }

            if (memberType.IsValueType)
            {
                return DataTypeClass.ValueType;
            }

            Attribute? nullableAttribute = GetNullableAttribute(memberInfo);

            if (nullableAttribute == null)
            {
                return HasNullableContextAttribute(memberInfo) ? DataTypeClass.NullableReferenceType : DataTypeClass.NonNullableReferenceType;
            }

            return HasNullableFlag(nullableAttribute) ? DataTypeClass.NonNullableReferenceType : DataTypeClass.NullableReferenceType;
        }

        private static Attribute? GetNullableAttribute(MemberInfo memberInfo)
        {
            Attribute? nullableAttribute = memberInfo.GetCustomAttributes()
                .FirstOrDefault(attr => string.Equals(attr.GetType().FullName, NullableAttributeFullTypeName));

            return nullableAttribute;
        }

        private static bool HasNullableContextAttribute(MemberInfo memberInfo)
        {
            if (memberInfo.DeclaringType?.DeclaringType == null)
            {
                return false;
            }

            Type[] declaringTypes = memberInfo.DeclaringType is { IsNested: true }
                ? new[]
                {
                    memberInfo.DeclaringType,
                    memberInfo.DeclaringType.DeclaringType
                }
                : new[]
                {
                    memberInfo.DeclaringType
                };

            foreach (Type type in declaringTypes)
            {
                var attributes = (IEnumerable<object>)type.GetCustomAttributes(false);

                object? nullableContext = attributes.FirstOrDefault(attr => string.Equals(attr.GetType().FullName, NullableContextAttributeFullTypeName));

                if (nullableContext != null)
                {
                    return nullableContext.GetType().GetField(FlagFieldName) is { } field && field.GetValue(nullableContext) is byte and 1;
                }
            }

            return false;
        }

        private static bool HasNullableFlag(Attribute nullableAttribute)
        {
            FieldInfo? fieldInfo = nullableAttribute.GetType().GetField(NullableFlagsFieldName);
            return fieldInfo is { } nullableFlagsField && nullableFlagsField.GetValue(nullableAttribute) is byte[] { Length: >= 1 } flags && flags[0] == 1;
        }
    }
}
