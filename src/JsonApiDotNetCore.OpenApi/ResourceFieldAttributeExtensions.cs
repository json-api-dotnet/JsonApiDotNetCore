using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi
{
    internal static class ResourceFieldAttributeExtensions
    {
        public static bool IsNullable(this ResourceFieldAttribute source)
        {
            TypeCategory fieldTypeCategory = source.Property.GetTypeCategory();
            bool hasRequiredAttribute = source.Property.HasAttribute<RequiredAttribute>();

            return fieldTypeCategory switch
            {
                TypeCategory.NonNullableReferenceType or TypeCategory.ValueType => false,
                TypeCategory.NullableReferenceType or TypeCategory.NullableValueType => !hasRequiredAttribute,
                _ => throw new UnreachableCodeException()
            };
        }
    }
}
