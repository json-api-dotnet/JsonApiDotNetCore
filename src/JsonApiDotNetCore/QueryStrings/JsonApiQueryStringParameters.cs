using JsonApiDotNetCore.Controllers.Annotations;

namespace JsonApiDotNetCore.QueryStrings;

/// <summary>
/// Lists query string parameters used by <see cref="DisableQueryStringAttribute" />.
/// </summary>
[Flags]
public enum JsonApiQueryStringParameters
{
    None = 0,
    Filter = 1,
    Sort = 1 << 1,
    Include = 1 << 2,
    Page = 1 << 3,
    Fields = 1 << 4,
    All = Filter | Sort | Include | Page | Fields
}
