using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Links;

internal static class LinkTypesExtensions
{
    public static IEnumerable<string> ToPropertyNames(this LinkTypes linkTypes)
    {
        if (linkTypes.HasFlag(LinkTypes.Self))
        {
            yield return "self";
        }

        if (linkTypes.HasFlag(LinkTypes.Related))
        {
            yield return "related";
        }

        if (linkTypes.HasFlag(LinkTypes.DescribedBy))
        {
            yield return "describedby";
        }

        if (linkTypes.HasFlag(LinkTypes.Pagination))
        {
            yield return "first";
            yield return "last";
            yield return "prev";
            yield return "next";
        }
    }
}
