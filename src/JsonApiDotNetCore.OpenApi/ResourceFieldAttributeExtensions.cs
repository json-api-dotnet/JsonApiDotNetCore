using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JsonApiDotNetCore.Resources.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi;

internal static class ResourceFieldAttributeExtensions
{
    private static readonly NullabilityInfoContext NullabilityInfoContext = new();

    public static bool IsNullable(this ResourceFieldAttribute source)
    {
        bool hasRequiredAttribute = source.Property.HasAttribute<RequiredAttribute>();

        if (hasRequiredAttribute)
        {
            // Reflects the following cases, independent of NRT setting
            //      `[Required] int? Number` => not nullable
            //      `[Required] int Number` => not nullable
            //      `[Required] string Text` => not nullable
            //      `[Required] string? Text` => not nullable
            //      `[Required] string Text` => not nullable
            return false;
        }

        NullabilityInfo nullabilityInfo = NullabilityInfoContext.Create(source.Property);

        //  Reflects the following cases:
        //    Independent of NRT:
        //      `int? Number` => nullable
        //      `int Number` => not nullable
        //    If NRT is enabled:
        //      `string? Text` => nullable
        //      `string Text` => not nullable
        //    If NRT is disabled:
        //      `string Text` => nullable
        return nullabilityInfo.ReadState is not NullabilityState.NotNull;
    }
}
